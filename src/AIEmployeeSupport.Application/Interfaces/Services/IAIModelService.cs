using AIEmployeeSupport.Application.DTOs.AI;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IAIModelService
{
    Task<IEnumerable<AIModelDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AIModelDto>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<AIModelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AIModelDto> CreateAsync(CreateAIModelRequest request, CancellationToken cancellationToken = default);
    Task<AIModelDto> UpdateAsync(Guid id, UpdateAIModelRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
