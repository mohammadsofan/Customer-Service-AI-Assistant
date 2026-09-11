using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IAIModelRepository
{
    Task<IEnumerable<AIModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AIModel>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<AIModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AIModel> CreateAsync(AIModel model, CancellationToken cancellationToken = default);
    Task UpdateAsync(AIModel model, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
