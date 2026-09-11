using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IKnowledgeScenarioRepository
{
    Task<(IEnumerable<KnowledgeScenario> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        ScenarioStatus? status = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default);
    Task<KnowledgeScenario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<KnowledgeScenario>> GetByStatusAsync(ScenarioStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<KnowledgeScenario>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<KnowledgeScenario> CreateAsync(KnowledgeScenario scenario, CancellationToken cancellationToken = default);
    Task UpdateAsync(KnowledgeScenario scenario, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
