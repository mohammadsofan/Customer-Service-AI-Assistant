using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IAIRequestLogRepository
{
    Task CreateAsync(AIRequestLog requestLog, CancellationToken cancellationToken = default);
    Task<IEnumerable<AIRequestLog>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, (string? ProviderName, string? ModelName, long? ModelDurationMs)>> GetQuestionAIModelInfoAsync(IEnumerable<Guid> questionIds, CancellationToken cancellationToken = default);
}
