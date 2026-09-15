using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

public class TwoStageRAGRetrievalTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TwoStageRAGRetrievalTests(CustomWebApplicationFactory factory)
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

    private async Task<(KnowledgeScenario Scenario, KnowledgeEmbedding Embedding)> SeedScenarioWithStepsAsync(
        string scenarioName,
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
            Name = scenarioName,
            Description = "Description for " + scenarioName,
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
                    StepText = "Step 1 for " + scenarioName,
                    Description = "Detailed step description",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    StepOrder = 2,
                    StepText = "Step 2 for " + scenarioName,
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
            Content = $"{scenario.Name}\n{scenario.Description}",
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
    public async Task SearchSimilarAsync_TwoStageRetrieval_ReturnsFullyHydratedTopKResults()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        var (scenario, _) = await SeedScenarioWithStepsAsync("Active TwoStage Test Scenario", ScenarioStatus.Active, EmbeddingStatus.Ready);

        var queryVector = CreateTestVector(1.0f);
        var results = (await repo.SearchSimilarAsync(queryVector, topK: 5, threshold: 0.5)).ToList();

        results.Should().NotBeEmpty();
        var match = results.FirstOrDefault(r => r.Embedding.ScenarioId == scenario.Id);
        match.Embedding.Should().NotBeNull();
        match.Embedding.Scenario.Should().NotBeNull();
        match.Embedding.Scenario.Name.Should().Be("Active TwoStage Test Scenario");
        match.Embedding.Scenario.Category.Should().NotBeNull();
        match.Embedding.Scenario.ResolutionSteps.Should().HaveCount(2);
        match.Embedding.Scenario.ResolutionSteps.First().StepText.Should().Contain("Step 1");
        match.Similarity.Should().BeGreaterThanOrEqualTo(0.99);
    }

    [Fact]
    public async Task SearchSimilarAsync_TwoStageRetrieval_PreservesSimilarityOrdering()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        // Vector 1: identical direction (similarity = 1.0)
        var v1 = CreateTestVector(1.0f);
        var (s1, _) = await SeedScenarioWithStepsAsync("High Match", ScenarioStatus.Active, EmbeddingStatus.Ready, v1);

        // Vector 2: partially aligned
        var v2Floats = new float[] { 1.0f, 1.0f, 0.0f, 0.0f };
        var v2 = new byte[v2Floats.Length * sizeof(float)];
        Buffer.BlockCopy(v2Floats, 0, v2, 0, v2.Length);
        var (s2, _) = await SeedScenarioWithStepsAsync("Medium Match", ScenarioStatus.Active, EmbeddingStatus.Ready, v2);

        var queryVector = CreateTestVector(1.0f);
        var results = (await repo.SearchSimilarAsync(queryVector, topK: 10, threshold: 0.1)).ToList();

        var s1Index = results.FindIndex(r => r.Embedding.ScenarioId == s1.Id);
        var s2Index = results.FindIndex(r => r.Embedding.ScenarioId == s2.Id);

        s1Index.Should().BeGreaterThanOrEqualTo(0);
        s2Index.Should().BeGreaterThanOrEqualTo(0);
        s1Index.Should().BeLessThan(s2Index); // Higher similarity comes first
    }

    [Fact]
    public async Task SearchSimilarAsync_TwoStageRetrieval_ExcludesDraftAndArchivedScenarios()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        var (draftScenario, _) = await SeedScenarioWithStepsAsync("Draft Scenario", ScenarioStatus.Draft, EmbeddingStatus.Ready);
        var (archivedScenario, _) = await SeedScenarioWithStepsAsync("Archived Scenario", ScenarioStatus.Archived, EmbeddingStatus.Ready);

        var queryVector = CreateTestVector(1.0f);
        var results = (await repo.SearchSimilarAsync(queryVector, topK: 20, threshold: 0.1)).ToList();

        results.Should().NotContain(r => r.Embedding.ScenarioId == draftScenario.Id);
        results.Should().NotContain(r => r.Embedding.ScenarioId == archivedScenario.Id);
    }

    [Fact]
    public async Task SearchSimilarAsync_TwoStageRetrieval_RespectsCancellationToken()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-canceled

        var queryVector = CreateTestVector(1.0f);
        Func<Task> act = async () => await repo.SearchSimilarAsync(queryVector, topK: 5, threshold: 0.5, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private async Task<(KnowledgeScenario Scenario, KnowledgeEmbedding Embedding)> SeedScenarioWithKeywordsAndStepsAsync(
        string scenarioName,
        string description,
        List<string> keywordNames,
        ScenarioStatus scenarioStatus,
        EmbeddingStatus embeddingStatus)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();

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
            Name = scenarioName,
            Description = description,
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
                    StepText = "ادخل رقم المشترك على TCRM",
                    Description = "شرح تفصيلي",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };

        foreach (var kw in keywordNames)
        {
            var keywordEntity = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = kw, CreatedAt = DateTime.UtcNow };
            db.KnowledgeKeywords.Add(keywordEntity);
            scenario.ScenarioKeywords.Add(new ScenarioKeyword { ScenarioId = scenario.Id, KeywordId = keywordEntity.Id });
        }

        db.KnowledgeScenarios.Add(scenario);

        var content = $"Title: {scenario.Name}\nDescription: {scenario.Description}\nKeywords: {string.Join(", ", keywordNames)}\nSteps:\n1. ادخل رقم المشترك على TCRM";
        var vectorFloats = await embeddingService.GenerateEmbeddingAsync(content);
        var vectorBytes = new byte[vectorFloats.Length * sizeof(float)];
        Buffer.BlockCopy(vectorFloats, 0, vectorBytes, 0, vectorBytes.Length);

        var embedding = new KnowledgeEmbedding
        {
            Id = Guid.NewGuid(),
            ScenarioId = scenario.Id,
            Content = content,
            Embedding = vectorBytes,
            Status = embeddingStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.KnowledgeEmbeddings.Add(embedding);

        await db.SaveChangesAsync();
        return (scenario, embedding);
    }

    [Fact]
    public async Task SearchSimilarAsync_ColloquialArabicInvoiceQuery_MatchesActiveScenarioAboveThreshold()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        var invoiceKeywords = new List<string>
        {
            "فواتير الجوال", "تفاصيل الفاتورة", "فاتورة", "قديش الفاتورة", "بده يعرف فاتورته", "فاتورته عالجوال"
        };

        var (scenario, _) = await SeedScenarioWithKeywordsAndStepsAsync(
            "فحص فواتير الجوال",
            "فحص فواتير الجوال ومعرفة قيمة الفواتير المستحقة",
            invoiceKeywords,
            ScenarioStatus.Active,
            EmbeddingStatus.Ready);

        var query = "المشترك بده يعرف قديش فاتورته عالجوال كيف افحصله؟";
        var queryFloats = await embeddingService.GenerateEmbeddingAsync(query);
        var queryBytes = new byte[queryFloats.Length * sizeof(float)];
        Buffer.BlockCopy(queryFloats, 0, queryBytes, 0, queryBytes.Length);

        var results = (await repo.SearchSimilarAsync(queryBytes, topK: 5, threshold: 0.25, queryText: query)).ToList();

        results.Should().NotBeEmpty();
        var top = results.First();
        top.Embedding.ScenarioId.Should().Be(scenario.Id);
        top.Similarity.Should().BeGreaterThanOrEqualTo(0.25);
    }

    [Fact]
    public async Task SearchSimilarAsync_UnrelatedQuery_IsSafelyDroppedBelowThreshold()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        var (scenario, _) = await SeedScenarioWithKeywordsAndStepsAsync(
            "فحص فواتير الجوال",
            "فحص فواتير الجوال ومعرفة قيمة الفواتير المستحقة",
            new List<string> { "فواتير الجوال", "فاتورة" },
            ScenarioStatus.Active,
            EmbeddingStatus.Ready);

        var query = "كيف أغير كلمة المرور للحساب؟";
        var queryFloats = await embeddingService.GenerateEmbeddingAsync(query);
        var queryBytes = new byte[queryFloats.Length * sizeof(float)];
        Buffer.BlockCopy(queryFloats, 0, queryBytes, 0, queryBytes.Length);

        var results = (await repo.SearchSimilarAsync(queryBytes, topK: 5, threshold: 0.25, queryText: query)).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchSimilarAsync_DraftScenarioWithMatchingKeywords_IsStrictlyExcluded()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        var (draftScenario, _) = await SeedScenarioWithKeywordsAndStepsAsync(
            "سيناريو مسودة للفواتير",
            "فحص فواتير الجوال",
            new List<string> { "فواتير الجوال", "فاتورة", "قديش الفاتورة" },
            ScenarioStatus.Draft,
            EmbeddingStatus.Ready);

        var query = "المشترك بده يعرف قديش فاتورته عالجوال كيف افحصله؟";
        var queryFloats = await embeddingService.GenerateEmbeddingAsync(query);
        var queryBytes = new byte[queryFloats.Length * sizeof(float)];
        Buffer.BlockCopy(queryFloats, 0, queryBytes, 0, queryBytes.Length);

        var results = (await repo.SearchSimilarAsync(queryBytes, topK: 5, threshold: 0.25, queryText: query)).ToList();

        results.Should().NotContain(r => r.Embedding.ScenarioId == draftScenario.Id);
    }

    [Fact]
    public async Task SearchSimilarAsync_RewrittenQuery_ExcludesDraftAndArchivedScenarios()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var repo = new KnowledgeEmbeddingRepository(db, NullLogger<KnowledgeEmbeddingRepository>.Instance);

        var (draftScenario, _) = await SeedScenarioWithKeywordsAndStepsAsync(
            "سيناريو مسودة فحص فواتير",
            "فحص فواتير الجوال",
            new List<string> { "فواتير الجوال", "فاتورة" },
            ScenarioStatus.Draft,
            EmbeddingStatus.Ready);

        var (archivedScenario, _) = await SeedScenarioWithKeywordsAndStepsAsync(
            "سيناريو مؤرشف فحص فواتير",
            "فحص فواتير الجوال",
            new List<string> { "فواتير الجوال", "فاتورة" },
            ScenarioStatus.Archived,
            EmbeddingStatus.Ready);

        var (activeScenario, _) = await SeedScenarioWithKeywordsAndStepsAsync(
            "فحص فواتير الجوال",
            "فحص فواتير الجوال ومعرفة قيمة الفواتير",
            new List<string> { "فواتير الجوال", "فاتورة" },
            ScenarioStatus.Active,
            EmbeddingStatus.Ready);

        // Simulate rewritten query from LLM
        var rewrittenQuery = "فحص فواتير الجوال";
        var queryFloats = await embeddingService.GenerateEmbeddingAsync(rewrittenQuery);
        var queryBytes = new byte[queryFloats.Length * sizeof(float)];
        Buffer.BlockCopy(queryFloats, 0, queryBytes, 0, queryBytes.Length);

        var results = (await repo.SearchSimilarAsync(queryBytes, topK: 10, threshold: 0.25, queryText: rewrittenQuery)).ToList();

        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.Embedding.ScenarioId == activeScenario.Id);
        results.Should().NotContain(r => r.Embedding.ScenarioId == draftScenario.Id);
        results.Should().NotContain(r => r.Embedding.ScenarioId == archivedScenario.Id);
    }
}
