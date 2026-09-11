using AIEmployeeSupport.Application.DTOs.AI;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IAIProviderService
{
    Task<IEnumerable<AIProviderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AIProviderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AIProviderDto> CreateAsync(CreateAIProviderRequest request, CancellationToken cancellationToken = default);
    Task<AIProviderDto> UpdateAsync(Guid id, UpdateAIProviderRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProviderTestResult> TestConnectionAsync(Guid id, CancellationToken cancellationToken = default);
}
