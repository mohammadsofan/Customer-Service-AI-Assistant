using AIEmployeeSupport.Application.DTOs.AI;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IAIConfigurationService
{
    Task<AIConfigurationDto?> GetAsync(CancellationToken cancellationToken = default);
    Task<AIConfigurationDto> UpdateAsync(Guid userId, UpdateAIConfigurationRequest request, CancellationToken cancellationToken = default);
}
