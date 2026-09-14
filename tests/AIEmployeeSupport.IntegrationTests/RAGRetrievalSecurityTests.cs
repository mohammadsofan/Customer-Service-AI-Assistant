using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIEmployeeSupport.Application.DTOs.Support;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using AIEmployeeSupport.Infrastructure.Persistence;
using AIEmployeeSupport.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AIEmployeeSupport.IntegrationTests;

public class RAGRetrievalSecurityTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RAGRetrievalSecurityTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static byte[] CreateTestVector(float value = 1.0f, int dimensions = 4)
    {
        var floats = new float[dimensions];
        for (int i = 0; i < dimensions; i++) floats[i] = value;
        var bytes = new byte[dimensions * sizeof(float)];
        Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    private async Task<(KnowledgeScenario Scenario, KnowledgeEmbedding Embedding)> SeedScenarioWithEmbeddingAsync(
        ScenarioStatus scenarioStatus,
        EmbeddingStatus embeddingStatus,
        byte[]? vector = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = new KnowledgeCategory
        {
            Id = Guid.NewGuid(),
            Name = $"Category_{Guid.NewGuid():N}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.KnowledgeCategories.Add(category);

        var scenario = new KnowledgeScenario
        {
            Id = Guid.NewGuid(),
            Name = $"Scenario_{scenarioStatus}_{Guid.NewGuid():N}",
            Description = "Test scenario description",
            CategoryId = category.Id,
            Status = scenarioStatus,
            CreatedBy = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ResolutionSteps = new List<ResolutionStep>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    StepOrder = 1,
                    StepText = "Step 1 text",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };
        db.KnowledgeScenarios.Add(scenario);

        var embedding = new KnowledgeEmbedding
        {
            Id = Guid.NewGuid(),
            ScenarioId = scenario.Id,
            Content = "Test content",
            Embedding = vector ?? CreateTestVector(1.0f),
            Status = embeddingStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.KnowledgeEmbeddings.Add(embedding);

        await db.SaveChangesAsync();
        return (scenario, embedding);
    }

    [Fact]
    public async Task SearchSimilarAsync_ActiveScenarioAndReadyEmbedding_IsReturned()
    {
        // Arrange
        var testVector = CreateTestVector(1.0f);
        var (scenario, _) = await SeedScenarioWithEmbeddingAsync(ScenarioStatus.Active, EmbeddingStatus.Ready, testVector);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        // Act
        var results = (await repo.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8)).ToList();

        // Assert
        results.Should().NotBeEmpty();
        results.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeTrue();
    }

    [Theory]
    [InlineData(ScenarioStatus.Draft)]
    [InlineData(ScenarioStatus.Archived)]
    [InlineData(ScenarioStatus.Inactive)]
    public async Task SearchSimilarAsync_NonActiveScenario_WithReadyEmbedding_IsExcluded(ScenarioStatus nonActiveStatus)
    {
        // Arrange
        var testVector = CreateTestVector(1.0f);
        var (scenario, _) = await SeedScenarioWithEmbeddingAsync(nonActiveStatus, EmbeddingStatus.Ready, testVector);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        // Act
        var results = (await repo.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8)).ToList();

        // Assert: Must NOT be returned by the database query
        results.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeFalse();
    }

    [Theory]
    [InlineData(EmbeddingStatus.Pending)]
    [InlineData(EmbeddingStatus.Failed)]
    public async Task SearchSimilarAsync_ActiveScenario_WithNonReadyEmbedding_IsExcluded(EmbeddingStatus nonReadyStatus)
    {
        // Arrange
        var testVector = CreateTestVector(1.0f);
        var (scenario, _) = await SeedScenarioWithEmbeddingAsync(ScenarioStatus.Active, nonReadyStatus, testVector);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        // Act
        var results = (await repo.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8)).ToList();

        // Assert: Must NOT be returned because embedding is not Ready
        results.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeFalse();
    }

    [Fact]
    public async Task SearchSimilarAsync_StatusTransition_ActiveToDraft_ImmediatelyExcluded()
    {
        // Arrange: Start with Active + Ready
        var testVector = CreateTestVector(1.0f);
        var (scenario, _) = await SeedScenarioWithEmbeddingAsync(ScenarioStatus.Active, EmbeddingStatus.Ready, testVector);

        using (var scope1 = _factory.Services.CreateScope())
        {
            var db1 = scope1.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repo1 = new KnowledgeEmbeddingRepository(db1, NullLogger<KnowledgeEmbeddingRepository>.Instance);

            var initialResults = await repo1.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8);
            initialResults.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeTrue();
        }

        // Transition: Active -> Draft
        using (var scope2 = _factory.Services.CreateScope())
        {
            var db2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var sc = await db2.KnowledgeScenarios.FindAsync(scenario.Id);
            sc!.Status = ScenarioStatus.Draft;
            await db2.SaveChangesAsync();
        }

        // Act: Search again
        using (var scope3 = _factory.Services.CreateScope())
        {
            var db3 = scope3.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repo3 = new KnowledgeEmbeddingRepository(db3, NullLogger<KnowledgeEmbeddingRepository>.Instance);

            var subsequentResults = await repo3.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8);

            // Assert: Must immediately be excluded without altering the embedding
            subsequentResults.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeFalse();
        }
    }

    [Fact]
    public async Task SearchSimilarAsync_StatusTransition_ActiveToArchived_ImmediatelyExcluded()
    {
        // Arrange: Start with Active + Ready
        var testVector = CreateTestVector(1.0f);
        var (scenario, _) = await SeedScenarioWithEmbeddingAsync(ScenarioStatus.Active, EmbeddingStatus.Ready, testVector);

        // Transition: Active -> Archived
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var sc = await db.KnowledgeScenarios.FindAsync(scenario.Id);
            sc!.Status = ScenarioStatus.Archived;
            await db.SaveChangesAsync();
        }

        // Act: Search
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

            var results = await repo.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8);

            // Assert: Must immediately be excluded
            results.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeFalse();
        }
    }

    [Fact]
    public async Task SearchSimilarAsync_StatusTransition_DraftToActive_ImmediatelyIncluded()
    {
        // Arrange: Start with Draft + Ready
        var testVector = CreateTestVector(1.0f);
        var (scenario, _) = await SeedScenarioWithEmbeddingAsync(ScenarioStatus.Draft, EmbeddingStatus.Ready, testVector);

        // Verify initially excluded
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);
            var results = await repo.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8);
            results.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeFalse();
        }

        // Transition: Draft -> Active
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var sc = await db.KnowledgeScenarios.FindAsync(scenario.Id);
            sc!.Status = ScenarioStatus.Active;
            await db.SaveChangesAsync();
        }

        // Act: Search again
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

            var results = await repo.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8);

            // Assert: Must now be included
            results.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeTrue();
        }
    }

    [Fact]
    public async Task SearchSimilarAsync_StatusTransition_ArchivedToActive_ImmediatelyIncluded()
    {
        // Arrange: Start with Archived + Ready
        var testVector = CreateTestVector(1.0f);
        var (scenario, _) = await SeedScenarioWithEmbeddingAsync(ScenarioStatus.Archived, EmbeddingStatus.Ready, testVector);

        // Verify initially excluded
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);
            var results = await repo.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8);
            results.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeFalse();
        }

        // Transition: Archived -> Active
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var sc = await db.KnowledgeScenarios.FindAsync(scenario.Id);
            sc!.Status = ScenarioStatus.Active;
            await db.SaveChangesAsync();
        }

        // Act: Search again
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

            var results = await repo.SearchSimilarAsync(testVector, topK: 5, threshold: 0.8);

            // Assert: Must now be included
            results.Any(r => r.Embedding.ScenarioId == scenario.Id).Should().BeTrue();
        }
    }
}
