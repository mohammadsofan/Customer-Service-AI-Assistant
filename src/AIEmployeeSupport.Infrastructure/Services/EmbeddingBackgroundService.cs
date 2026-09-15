using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AIEmployeeSupport.Application.Common.Settings;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using AIEmployeeSupport.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIEmployeeSupport.Infrastructure.Services;

public class EmbeddingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEmbeddingQueue _embeddingQueue;
    private readonly IOptions<EmbeddingSettings> _settings;
    private readonly ILogger<EmbeddingBackgroundService> _logger;

    public EmbeddingBackgroundService(
        IServiceProvider serviceProvider,
        IEmbeddingQueue embeddingQueue,
        IOptions<EmbeddingSettings> settings,
        ILogger<EmbeddingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _embeddingQueue = embeddingQueue;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Embedding Background Service is starting with event-driven channel and durable recovery sweep.");

        // ── 1. Startup Recovery Sweep ─────────────────────────────────────────
        // Immediately sweep database for any stranded Pending embeddings from before
        // application restart or worker interruption.
        try
        {
            await RunRecoverySweepAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during initial embedding recovery sweep on startup.");
        }

        // ── 2. Run Fast-Path Consumer & Periodic Recovery Sweep in Parallel ───
        var fastPathTask = ProcessChannelWorkAsync(stoppingToken);
        var recoverySweepTask = RunPeriodicRecoverySweepAsync(stoppingToken);

        await Task.WhenAll(fastPathTask, recoverySweepTask);
    }

    private async Task ProcessChannelWorkAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var scenarioId = await _embeddingQueue.DequeueAsync(stoppingToken);
                await ProcessSingleScenarioEmbeddingAsync(scenarioId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing embedding item from channel queue.");
            }
        }
    }

    private async Task RunPeriodicRecoverySweepAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = _settings.Value.RecoverySweepIntervalMinutes > 0
            ? _settings.Value.RecoverySweepIntervalMinutes
            : 2;

        var sweepInterval = TimeSpan.FromMinutes(intervalMinutes);
        using var timer = new PeriodicTimer(sweepInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("[Embedding Recovery] Running scheduled database reconciliation sweep.");
                await RunRecoverySweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during periodic embedding recovery sweep.");
            }
        }
    }

    private async Task RunRecoverySweepAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Recover scenarios missing an embedding record (only for Active or Draft scenarios per business rules)
        var scenariosWithoutEmbedding = await dbContext.KnowledgeScenarios
            .Where(s => (s.Status == ScenarioStatus.Active || s.Status == ScenarioStatus.Draft) &&
                        !dbContext.KnowledgeEmbeddings.Any(e => e.ScenarioId == s.Id))
            .Take(50)
            .ToListAsync(stoppingToken);

        if (scenariosWithoutEmbedding.Any())
        {
            _logger.LogInformation("[Embedding Recovery] Auto-creating missing embedding records for {Count} scenario(s).", scenariosWithoutEmbedding.Count);
            foreach (var s in scenariosWithoutEmbedding)
            {
                dbContext.KnowledgeEmbeddings.Add(new KnowledgeEmbedding
                {
                    Id = Guid.NewGuid(),
                    ScenarioId = s.Id,
                    Status = EmbeddingStatus.Pending,
                    Content = $"{s.Name}\n{s.Description}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await dbContext.SaveChangesAsync(stoppingToken);

            foreach (var s in scenariosWithoutEmbedding)
            {
                _embeddingQueue.QueueEmbeddingWork(s.Id);
            }
        }

        // Recover stranded Pending embeddings in the database
        var pendingScenarioIds = await dbContext.KnowledgeEmbeddings
            .Where(e => e.Status == EmbeddingStatus.Pending)
            .Select(e => e.ScenarioId)
            .Take(50)
            .ToListAsync(stoppingToken);

        if (pendingScenarioIds.Any())
        {
            _logger.LogInformation("[Embedding Recovery] Found {Count} stranded Pending embedding(s). Enqueuing to channel.", pendingScenarioIds.Count);
            foreach (var scenarioId in pendingScenarioIds)
            {
                _embeddingQueue.QueueEmbeddingWork(scenarioId);
            }
        }
    }

    private async Task ProcessSingleScenarioEmbeddingAsync(Guid scenarioId, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var embedding = await dbContext.KnowledgeEmbeddings
            .Include(e => e.Scenario)
                .ThenInclude(s => s.Category)
            .Include(e => e.Scenario)
                .ThenInclude(s => s.ScenarioKeywords)
                    .ThenInclude(sk => sk.Keyword)
            .Include(e => e.Scenario)
                .ThenInclude(s => s.ResolutionSteps)
            .FirstOrDefaultAsync(e => e.ScenarioId == scenarioId, stoppingToken);

        if (embedding == null)
        {
            _logger.LogWarning("[Embedding] No embedding record found for Scenario ID: {ScenarioId}. Skipping.", scenarioId);
            return;
        }

        // Idempotency: skip if already processed and no longer Pending
        if (embedding.Status != EmbeddingStatus.Pending)
        {
            _logger.LogDebug("[Embedding] Scenario ID {ScenarioId} embedding is already in {Status} state. Skipping duplicate work.", scenarioId, embedding.Status);
            return;
        }

        if (embedding.Scenario == null)
        {
            _logger.LogWarning("[Embedding] Parent scenario for embedding {EmbeddingId} was removed. Skipping.", embedding.Id);
            return;
        }

        try
        {
            _logger.LogInformation("[Embedding] Vectorizing scenario: '{ScenarioName}' (ID: {ScenarioId})", embedding.Scenario.Name, scenarioId);

            var sb = new StringBuilder();
            sb.AppendLine($"Title: {embedding.Scenario.Name}");
            if (embedding.Scenario.Category != null && !string.IsNullOrWhiteSpace(embedding.Scenario.Category.Name))
            {
                sb.AppendLine($"Category: {embedding.Scenario.Category.Name}");
            }
            sb.AppendLine($"Description: {embedding.Scenario.Description}");

            if (embedding.Scenario.ScenarioKeywords != null && embedding.Scenario.ScenarioKeywords.Any())
            {
                sb.AppendLine("Keywords: " + string.Join(", ", embedding.Scenario.ScenarioKeywords.Select(k => k.Keyword?.Name).Where(k => !string.IsNullOrEmpty(k))));
            }

            if (embedding.Scenario.ResolutionSteps != null && embedding.Scenario.ResolutionSteps.Any())
            {
                sb.AppendLine("Steps:");
                var orderedSteps = embedding.Scenario.ResolutionSteps.OrderBy(s => s.StepOrder).ToList();
                foreach (var step in orderedSteps)
                {
                    if (!string.IsNullOrWhiteSpace(step.Description))
                    {
                        sb.AppendLine($"{step.StepOrder}. {step.StepText} - {step.Description}");
                    }
                    else
                    {
                        sb.AppendLine($"{step.StepOrder}. {step.StepText}");
                    }
                }
            }

            var contentToEmbed = sb.ToString();
            embedding.Content = contentToEmbed;

            var vector = await embeddingService.GenerateEmbeddingAsync(contentToEmbed, stoppingToken);

            if (vector != null && vector.Length > 0)
            {
                var byteArray = new byte[vector.Length * 4];
                Buffer.BlockCopy(vector, 0, byteArray, 0, byteArray.Length);

                embedding.Embedding = byteArray;
                embedding.Status = EmbeddingStatus.Ready;
                embedding.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("[Embedding] Successfully generated embedding for Scenario ID: {ScenarioId}", scenarioId);
            }
            else
            {
                embedding.Status = EmbeddingStatus.Failed;
                embedding.UpdatedAt = DateTime.UtcNow;
                _logger.LogWarning("[Embedding] Generated empty vector for Scenario ID: {ScenarioId}", scenarioId);
            }

            await unitOfWork.SaveChangesAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Embedding] Exception while generating embedding for Scenario ID: {ScenarioId}", scenarioId);
            try
            {
                embedding.Status = EmbeddingStatus.Failed;
                embedding.UpdatedAt = DateTime.UtcNow;
                await unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "[Embedding] Failed to persist Failed status for Scenario ID: {ScenarioId}", scenarioId);
            }
        }
    }
}
