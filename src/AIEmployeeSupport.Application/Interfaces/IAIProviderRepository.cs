using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IAIProviderRepository
{
    Task<IEnumerable<AIProvider>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AIProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AIProvider>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AIProvider>> GetByPriorityAsync(CancellationToken cancellationToken = default);
    Task<AIProvider> CreateAsync(AIProvider provider, CancellationToken cancellationToken = default);
    Task UpdateAsync(AIProvider provider, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
