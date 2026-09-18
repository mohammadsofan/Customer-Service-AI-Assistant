using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class AIConfigurationRepository : IAIConfigurationRepository
{
    private readonly ApplicationDbContext _context;

    public AIConfigurationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AIConfiguration?> GetAsync(CancellationToken cancellationToken = default)
        => await _context.AIConfigurations.AsNoTracking()
            .Include(c => c.ActiveProvider)
            .Include(c => c.ActiveModel)
            .Include(c => c.ActiveEmbeddingProvider)
            .Include(c => c.ActiveEmbeddingModel)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task UpdateAsync(AIConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AIConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (existing != null)
        {
            existing.ActiveProviderId = configuration.ActiveProviderId;
            existing.ActiveModelId = configuration.ActiveModelId;
            existing.ActiveEmbeddingProviderId = configuration.ActiveEmbeddingProviderId;
            existing.ActiveEmbeddingModelId = configuration.ActiveEmbeddingModelId;
            existing.Temperature = configuration.Temperature;
            existing.MaxTokens = configuration.MaxTokens;
            existing.SimilarityThreshold = configuration.SimilarityThreshold;
            existing.TopK = configuration.TopK;
            existing.SystemPrompt = configuration.SystemPrompt;
            existing.EnableAutoFailover = configuration.EnableAutoFailover;
            existing.UpdatedBy = configuration.UpdatedBy;
            existing.UpdatedAt = configuration.UpdatedAt;
        }
        else
        {
            _context.AIConfigurations.Add(configuration);
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
