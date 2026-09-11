using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IResolutionStepRepository
{
    Task<IEnumerable<ResolutionStep>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken cancellationToken = default);
    Task<ResolutionStep> CreateAsync(ResolutionStep step, CancellationToken cancellationToken = default);
    Task UpdateAsync(ResolutionStep step, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ReorderStepsAsync(Guid scenarioId, IEnumerable<Guid> orderedStepIds, CancellationToken cancellationToken = default);
}
