using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IAIConfigurationRepository
{
    Task<AIConfiguration?> GetAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(AIConfiguration configuration, CancellationToken cancellationToken = default);
}
