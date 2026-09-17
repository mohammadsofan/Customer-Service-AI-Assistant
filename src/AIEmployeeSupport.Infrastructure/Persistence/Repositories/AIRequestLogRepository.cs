using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class AIRequestLogRepository : IAIRequestLogRepository
{
    private readonly ApplicationDbContext _context;

    public AIRequestLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(AIRequestLog requestLog, CancellationToken cancellationToken = default)
    {
        _context.AIRequestLogs.Add(requestLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<AIRequestLog>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default)
        => await _context.AIRequestLogs.AsNoTracking()
            .Where(r => r.QuestionId == questionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Dictionary<Guid, (string? ProviderName, string? ModelName)>> GetQuestionAIModelInfoAsync(
        IEnumerable<Guid> questionIds, CancellationToken cancellationToken = default)
    {
        var ids = questionIds.Distinct().ToList();
        if (!ids.Any()) return new Dictionary<Guid, (string? ProviderName, string? ModelName)>();

        var query = from log in _context.AIRequestLogs.AsNoTracking()
                    where ids.Contains(log.QuestionId)
                    join provider in _context.AIProviders.AsNoTracking() on log.ProviderId equals provider.Id into provGroup
                    from provider in provGroup.DefaultIfEmpty()
                    join model in _context.AIModels.AsNoTracking() on log.ModelId equals model.Id into modelGroup
                    from model in modelGroup.DefaultIfEmpty()
                    orderby log.CreatedAt descending
                    select new
                    {
                        log.QuestionId,
                        ProviderName = provider != null ? provider.Name : null,
                        ModelName = model != null ? model.ModelName : null
                    };

        var list = await query.ToListAsync(cancellationToken);
        var result = new Dictionary<Guid, (string? ProviderName, string? ModelName)>();

        foreach (var item in list)
        {
            if (!result.ContainsKey(item.QuestionId))
            {
                result[item.QuestionId] = (item.ProviderName, item.ModelName);
            }
        }

        return result;
    }
}
