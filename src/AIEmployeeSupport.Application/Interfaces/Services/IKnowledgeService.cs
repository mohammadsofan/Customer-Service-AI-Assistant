using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Knowledge;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IKnowledgeService
{
    Task<PaginatedResponse<ScenarioListDto>> GetScenariosAsync(PaginatedRequest request, ScenarioStatus? status = null, Guid? categoryId = null, CancellationToken cancellationToken = default);
    Task<ScenarioDetailDto?> GetScenarioByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ScenarioDetailDto> CreateScenarioAsync(Guid userId, CreateScenarioRequest request, CancellationToken cancellationToken = default);
    Task<ScenarioDetailDto> UpdateScenarioAsync(Guid scenarioId, Guid userId, UpdateScenarioRequest request, CancellationToken cancellationToken = default);
    Task DeleteScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid scenarioId, Guid userId, ScenarioStatus status, CancellationToken cancellationToken = default);
    Task ReindexAsync(Guid scenarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ScenarioVersionDto>> GetVersionsAsync(Guid scenarioId, CancellationToken cancellationToken = default);
}
