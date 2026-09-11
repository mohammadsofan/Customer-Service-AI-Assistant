using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IKnowledgeScenarioVersionRepository
{
    Task<IEnumerable<KnowledgeScenarioVersion>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken cancellationToken = default);
    Task<KnowledgeScenarioVersion> CreateAsync(KnowledgeScenarioVersion version, CancellationToken cancellationToken = default);
}
