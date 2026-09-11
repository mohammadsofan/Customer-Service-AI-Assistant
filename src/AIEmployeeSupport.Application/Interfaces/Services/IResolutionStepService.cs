using AIEmployeeSupport.Application.DTOs.Knowledge;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IResolutionStepService
{
    Task<IEnumerable<ResolutionStepDto>> GetByScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default);
    Task<ResolutionStepDto> AddAsync(Guid scenarioId, string stepText, CancellationToken cancellationToken = default);
    Task<ResolutionStepDto> UpdateAsync(Guid stepId, string stepText, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid stepId, CancellationToken cancellationToken = default);
    Task ReorderAsync(Guid scenarioId, IEnumerable<Guid> orderedStepIds, CancellationToken cancellationToken = default);
}
