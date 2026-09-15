using System.Text.Json;
using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Knowledge;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;

namespace AIEmployeeSupport.Application.Services;

public class KnowledgeService : IKnowledgeService
{
    private readonly IKnowledgeScenarioRepository _scenarioRepository;
    private readonly IKnowledgeCategoryRepository _categoryRepository;
    private readonly IKnowledgeKeywordRepository _keywordRepository;
    private readonly IKnowledgeEmbeddingRepository _embeddingRepository;
    private readonly IKnowledgeScenarioVersionRepository _versionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly IEmbeddingQueue _embeddingQueue;

    public KnowledgeService(
        IKnowledgeScenarioRepository scenarioRepository,
        IKnowledgeCategoryRepository categoryRepository,
        IKnowledgeKeywordRepository keywordRepository,
        IKnowledgeEmbeddingRepository embeddingRepository,
        IKnowledgeScenarioVersionRepository versionRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        IEmbeddingQueue embeddingQueue)
    {
        _scenarioRepository = scenarioRepository;
        _categoryRepository = categoryRepository;
        _keywordRepository = keywordRepository;
        _embeddingRepository = embeddingRepository;
        _versionRepository = versionRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _embeddingQueue = embeddingQueue;
    }

    public async Task<PaginatedResponse<ScenarioListDto>> GetScenariosAsync(PaginatedRequest request, ScenarioStatus? status = null, Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _scenarioRepository.GetAllAsync(request.Page, request.PageSize, status, categoryId, cancellationToken);
        
        var dtos = items.Select(s => new ScenarioListDto
        {
            Id = s.Id,
            Name = s.Name,
            CategoryName = s.Category?.Name ?? string.Empty,
            Status = s.Status.ToString(),
            KeywordCount = s.ScenarioKeywords?.Count ?? 0,
            StepCount = s.ResolutionSteps?.Count ?? 0,
            CreatedAt = s.CreatedAt
        }).ToList();

        return new PaginatedResponse<ScenarioListDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<ScenarioDetailDto?> GetScenarioByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(id, cancellationToken);
        if (scenario == null) return null;

        return new ScenarioDetailDto
        {
            Id = scenario.Id,
            Name = scenario.Name,
            Description = scenario.Description,
            CategoryId = scenario.CategoryId,
            CategoryName = scenario.Category?.Name ?? string.Empty,
            Status = scenario.Status.ToString(),
            Keywords = scenario.ScenarioKeywords?.Select(sk => sk.Keyword?.Name ?? string.Empty).Where(k => !string.IsNullOrEmpty(k)).ToList() ?? new List<string>(),
            ResolutionSteps = scenario.ResolutionSteps?.OrderBy(rs => rs.StepOrder).Select(rs => new ResolutionStepDto
            {
                Id = rs.Id,
                StepOrder = rs.StepOrder,
                StepText = rs.StepText,
                Description = rs.Description
            }).ToList() ?? new List<ResolutionStepDto>(),
            CreatedAt = scenario.CreatedAt,
            UpdatedAt = scenario.UpdatedAt
        };
    }

    public async Task<ScenarioDetailDto> CreateScenarioAsync(Guid userId, CreateScenarioRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null) throw new Exception("Category not found");

        var status = ScenarioStatus.Draft;
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<ScenarioStatus>(request.Status, ignoreCase: true, out var parsedStatus))
            {
                status = parsedStatus;
            }
            else if (request.Status == "نشط" || request.Status == "1")
            {
                status = ScenarioStatus.Active;
            }
        }
        else
        {
            status = ScenarioStatus.Active;
        }

        var hasValidSteps = (request.Steps != null && request.Steps.Any(s => !string.IsNullOrWhiteSpace(s.StepText)))
            || (request.ResolutionSteps != null && request.ResolutionSteps.Any(s => !string.IsNullOrWhiteSpace(s)));

        if (status == ScenarioStatus.Active && !hasValidSteps)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("ResolutionSteps", "يجب إضافة خطوة حل واحدة على الأقل عند تفعيل السيناريو")
            });
        }

        var scenario = new KnowledgeScenario
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Status = status,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var allKeywords = await _keywordRepository.GetAllAsync(cancellationToken);
        if (request.Keywords != null)
        {
            foreach (var keywordName in request.Keywords)
            {
                var keyword = allKeywords.FirstOrDefault(k => string.Equals(k.Name, keywordName, StringComparison.OrdinalIgnoreCase));
                if (keyword == null)
                {
                    keyword = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = keywordName, CreatedAt = DateTime.UtcNow };
                    await _keywordRepository.CreateAsync(keyword, cancellationToken);
                }
                scenario.ScenarioKeywords.Add(new ScenarioKeyword { ScenarioId = scenario.Id, KeywordId = keyword.Id });
            }
        }

        if (request.Steps != null && request.Steps.Any())
        {
            for (int i = 0; i < request.Steps.Count; i++)
            {
                scenario.ResolutionSteps.Add(new ResolutionStep
                {
                    Id = Guid.NewGuid(),
                    ScenarioId = scenario.Id,
                    StepOrder = i + 1,
                    StepText = request.Steps[i].StepText,
                    Description = request.Steps[i].Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
        else if (request.ResolutionSteps != null)
        {
            for (int i = 0; i < request.ResolutionSteps.Count; i++)
            {
                scenario.ResolutionSteps.Add(new ResolutionStep
                {
                    Id = Guid.NewGuid(),
                    ScenarioId = scenario.Id,
                    StepOrder = i + 1,
                    StepText = request.ResolutionSteps[i],
                    Description = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _scenarioRepository.CreateAsync(scenario, cancellationToken);

        var stepsSnapshot = scenario.ResolutionSteps.OrderBy(r => r.StepOrder).Select(r => new
        {
            r.StepOrder,
            r.StepText,
            r.Description
        });

        var version = new KnowledgeScenarioVersion
        {
            Id = Guid.NewGuid(),
            ScenarioId = scenario.Id,
            Version = 1,
            Name = scenario.Name,
            Description = scenario.Description,
            ResolutionStepsSnapshot = JsonSerializer.Serialize(stepsSnapshot),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        await _versionRepository.CreateAsync(version, cancellationToken);

        var embedding = new KnowledgeEmbedding
        {
            Id = Guid.NewGuid(),
            ScenarioId = scenario.Id,
            Content = $"{scenario.Name}\n{scenario.Description}",
            Status = EmbeddingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _embeddingRepository.UpsertAsync(embedding, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(userId, AuditAction.ScenarioCreated, "KnowledgeScenario", scenario.Id, null, cancellationToken);

        _embeddingQueue.QueueEmbeddingWork(scenario.Id);

        return await GetScenarioByIdAsync(scenario.Id, cancellationToken) ?? throw new Exception("Scenario creation failed.");
    }

    public async Task<ScenarioDetailDto> UpdateScenarioAsync(Guid scenarioId, Guid userId, UpdateScenarioRequest request, CancellationToken cancellationToken = default)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(scenarioId, cancellationToken);
        if (scenario == null) throw new Exception("Scenario not found");

        var hasValidSteps = (request.Steps != null && request.Steps.Any(s => !string.IsNullOrWhiteSpace(s.StepText)))
            || (request.ResolutionSteps != null && request.ResolutionSteps.Any(s => !string.IsNullOrWhiteSpace(s)));

        if (scenario.Status == ScenarioStatus.Active && !hasValidSteps)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("ResolutionSteps", "يجب إضافة خطوة حل واحدة على الأقل عند تفعيل السيناريو")
            });
        }

        scenario.Name = request.Name;
        scenario.Description = request.Description;
        scenario.CategoryId = request.CategoryId;
        scenario.UpdatedBy = userId;
        scenario.UpdatedAt = DateTime.UtcNow;

        scenario.ScenarioKeywords.Clear();
        var allKeywords = await _keywordRepository.GetAllAsync(cancellationToken);
        if (request.Keywords != null)
        {
            foreach (var keywordName in request.Keywords)
            {
                var keyword = allKeywords.FirstOrDefault(k => string.Equals(k.Name, keywordName, StringComparison.OrdinalIgnoreCase));
                if (keyword == null)
                {
                    keyword = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = keywordName, CreatedAt = DateTime.UtcNow };
                    await _keywordRepository.CreateAsync(keyword, cancellationToken);
                }
                scenario.ScenarioKeywords.Add(new ScenarioKeyword { ScenarioId = scenario.Id, KeywordId = keyword.Id });
            }
        }

        scenario.ResolutionSteps.Clear();
        if (request.Steps != null && request.Steps.Any())
        {
            for (int i = 0; i < request.Steps.Count; i++)
            {
                scenario.ResolutionSteps.Add(new ResolutionStep
                {
                    Id = Guid.NewGuid(),
                    ScenarioId = scenario.Id,
                    StepOrder = i + 1,
                    StepText = request.Steps[i].StepText,
                    Description = request.Steps[i].Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
        else if (request.ResolutionSteps != null)
        {
            for (int i = 0; i < request.ResolutionSteps.Count; i++)
            {
                scenario.ResolutionSteps.Add(new ResolutionStep
                {
                    Id = Guid.NewGuid(),
                    ScenarioId = scenario.Id,
                    StepOrder = i + 1,
                    StepText = request.ResolutionSteps[i],
                    Description = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _scenarioRepository.UpdateAsync(scenario, cancellationToken);

        var existingVersions = await _versionRepository.GetByScenarioIdAsync(scenarioId, cancellationToken);
        int nextVersion = (existingVersions != null && existingVersions.Any()) ? existingVersions.Max(v => v.Version) + 1 : 1;

        var stepsSnapshot = scenario.ResolutionSteps.OrderBy(r => r.StepOrder).Select(r => new
        {
            r.StepOrder,
            r.StepText,
            r.Description
        });

        var version = new KnowledgeScenarioVersion
        {
            Id = Guid.NewGuid(),
            ScenarioId = scenario.Id,
            Version = nextVersion,
            Name = scenario.Name,
            Description = scenario.Description,
            ResolutionStepsSnapshot = JsonSerializer.Serialize(stepsSnapshot),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        await _versionRepository.CreateAsync(version, cancellationToken);

        var embedding = await _embeddingRepository.GetByScenarioIdAsync(scenarioId, cancellationToken);
        if (embedding != null)
        {
            embedding.Content = $"{scenario.Name}\n{scenario.Description}";
            embedding.Status = EmbeddingStatus.Pending;
            embedding.UpdatedAt = DateTime.UtcNow;
            await _embeddingRepository.UpsertAsync(embedding, cancellationToken);
        }
        else
        {
            await _embeddingRepository.UpsertAsync(new KnowledgeEmbedding
            {
                Id = Guid.NewGuid(),
                ScenarioId = scenario.Id,
                Content = $"{scenario.Name}\n{scenario.Description}",
                Status = EmbeddingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _auditService.LogAsync(userId, AuditAction.ScenarioUpdated, "KnowledgeScenario", scenario.Id, null, cancellationToken);

        _embeddingQueue.QueueEmbeddingWork(scenario.Id);

        return await GetScenarioByIdAsync(scenario.Id, cancellationToken) ?? throw new Exception("Update failed.");
    }

    public async Task DeleteScenarioAsync(Guid scenarioId, CancellationToken cancellationToken = default)
    {
        await _scenarioRepository.DeleteAsync(scenarioId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(Guid.Empty, AuditAction.ScenarioArchived, "KnowledgeScenario", scenarioId, "Deleted", cancellationToken);
    }

    public async Task UpdateStatusAsync(Guid scenarioId, Guid userId, ScenarioStatus status, CancellationToken cancellationToken = default)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(scenarioId, cancellationToken);
        if (scenario == null) throw new Exception("Scenario not found");

        if (status == ScenarioStatus.Active)
        {
            if (string.IsNullOrWhiteSpace(scenario.Name) || 
                string.IsNullOrWhiteSpace(scenario.Description) || 
                scenario.CategoryId == Guid.Empty || 
                scenario.ResolutionSteps == null || 
                scenario.ResolutionSteps.Count == 0)
            {
                throw new Exception("Scenario must have a name, description, category, and at least one resolution step to be activated.");
            }
        }

        scenario.Status = status;
        scenario.UpdatedBy = userId;
        scenario.UpdatedAt = DateTime.UtcNow;

        await _scenarioRepository.UpdateAsync(scenario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var action = status == ScenarioStatus.Active ? AuditAction.ScenarioActivated : (status == ScenarioStatus.Archived ? AuditAction.ScenarioArchived : AuditAction.ScenarioUpdated);
        await _auditService.LogAsync(userId, action, "KnowledgeScenario", scenarioId, $"Status updated to {status}", cancellationToken);
    }

    public async Task ReindexAsync(Guid scenarioId, CancellationToken cancellationToken = default)
    {
        var embedding = await _embeddingRepository.GetByScenarioIdAsync(scenarioId, cancellationToken);
        if (embedding != null)
        {
            embedding.Status = EmbeddingStatus.Pending;
            embedding.UpdatedAt = DateTime.UtcNow;
            await _embeddingRepository.UpsertAsync(embedding, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _embeddingQueue.QueueEmbeddingWork(scenarioId);
        }
    }

    public async Task<IEnumerable<ScenarioVersionDto>> GetVersionsAsync(Guid scenarioId, CancellationToken cancellationToken = default)
    {
        var versions = await _versionRepository.GetByScenarioIdAsync(scenarioId, cancellationToken);
        return versions.Select(v => new ScenarioVersionDto
        {
            Id = v.Id,
            Version = v.Version,
            Name = v.Name,
            Description = v.Description,
            CreatedBy = v.CreatedBy.ToString(),
            CreatedAt = v.CreatedAt
        }).OrderByDescending(v => v.Version);
    }
}
