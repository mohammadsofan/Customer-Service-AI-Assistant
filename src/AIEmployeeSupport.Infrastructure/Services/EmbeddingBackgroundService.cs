using System.Text;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIEmployeeSupport.Infrastructure.Services;

public class EmbeddingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmbeddingBackgroundService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    public EmbeddingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<EmbeddingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Embedding Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEmbeddingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing pending embeddings.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingEmbeddingsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var embeddingRepo = scope.ServiceProvider.GetRequiredService<IKnowledgeEmbeddingRepository>();
        var scenarioRepo = scope.ServiceProvider.GetRequiredService<IKnowledgeScenarioRepository>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // We can't rely on a specific repository method for this, so we'll cast to get the DbContext
        // or just use a custom query if the interface doesn't have a GetPending() method.
        // Let's assume there is an IKnowledgeEmbeddingRepository, but wait, Task 3.1 didn't specify GetPending.
        // It specified: GetByScenarioId, Upsert, SearchSimilar.
        // So how do we get pending embeddings? I will use DbContext directly since it's Infrastructure layer.
        var dbContext = scope.ServiceProvider.GetRequiredService<AIEmployeeSupport.Infrastructure.Persistence.ApplicationDbContext>();
        
        var pendingEmbeddings = await dbContext.KnowledgeEmbeddings
            .Include(e => e.Scenario)
                .ThenInclude(s => s.ScenarioKeywords)
                    .ThenInclude(sk => sk.Keyword)
            .Include(e => e.Scenario)
                .ThenInclude(s => s.ResolutionSteps)
            .Where(e => e.Status == EmbeddingStatus.Pending)
            .Take(10) // Process in batches
            .ToListAsync(stoppingToken);

        if (!pendingEmbeddings.Any())
        {
            return;
        }

        foreach (var embedding in pendingEmbeddings)
        {
            try
            {
                _logger.LogInformation("Processing pending embedding for Scenario ID: {ScenarioId}", embedding.ScenarioId);

                // Concatenate content
                var sb = new StringBuilder();
                sb.AppendLine($"Title: {embedding.Scenario.Name}");
                sb.AppendLine($"Description: {embedding.Scenario.Description}");
                
                if (embedding.Scenario.ScenarioKeywords.Any())
                {
                    sb.AppendLine("Keywords: " + string.Join(", ", embedding.Scenario.ScenarioKeywords.Select(k => k.Keyword.Name)));
                }

                if (embedding.Scenario.ResolutionSteps.Any())
                {
                    sb.AppendLine("Steps:");
                    var orderedSteps = embedding.Scenario.ResolutionSteps.OrderBy(s => s.StepOrder).ToList();
                    foreach (var step in orderedSteps)
                    {
                        sb.AppendLine($"{step.StepOrder}. {step.StepText}");
                    }
                }

                var contentToEmbed = sb.ToString();
                embedding.Content = contentToEmbed;

                var vector = await embeddingService.GenerateEmbeddingAsync(contentToEmbed, stoppingToken);
                
                if (vector != null && vector.Length > 0)
                {
                    // Convert float[] to byte[] for storage
                    var byteArray = new byte[vector.Length * 4];
                    Buffer.BlockCopy(vector, 0, byteArray, 0, byteArray.Length);
                    
                    embedding.Embedding = byteArray;
                    embedding.Status = EmbeddingStatus.Ready;
                    embedding.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("Successfully generated embedding for Scenario ID: {ScenarioId}", embedding.ScenarioId);
                }
                else
                {
                    embedding.Status = EmbeddingStatus.Failed;
                    embedding.UpdatedAt = DateTime.UtcNow;
                    _logger.LogWarning("Failed to generate embedding vector for Scenario ID: {ScenarioId}", embedding.ScenarioId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while generating embedding for Scenario ID: {ScenarioId}", embedding.ScenarioId);
                embedding.Status = EmbeddingStatus.Failed;
                embedding.UpdatedAt = DateTime.UtcNow;
            }
        }

        await unitOfWork.SaveChangesAsync(stoppingToken);
    }
}
