using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.DTOs.Knowledge;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Services;

public class KeywordService : IKeywordService
{
    private readonly IKnowledgeKeywordRepository _keywordRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public KeywordService(IKnowledgeKeywordRepository keywordRepository, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _keywordRepository = keywordRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<IEnumerable<KeywordDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var keywords = await _keywordRepository.GetAllAsync(cancellationToken);
        return keywords.Select(k => new KeywordDto
        {
            Id = k.Id,
            Name = k.Name,
            CreatedAt = k.CreatedAt,
            ScenarioCount = k.ScenarioKeywords?.Count ?? 0
        });
    }

    public async Task<PaginatedResponse<KeywordDto>> GetAllAsync(PaginatedRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (items, totalCount) = await _keywordRepository.GetAllAsync(page, pageSize, request.Search, cancellationToken);

        var dtos = items.Select(k => new KeywordDto
        {
            Id = k.Id,
            Name = k.Name,
            CreatedAt = k.CreatedAt,
            ScenarioCount = k.ScenarioKeywords?.Count ?? 0
        }).ToList();

        return new PaginatedResponse<KeywordDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<KeywordDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var keyword = await _keywordRepository.GetByIdAsync(id, cancellationToken);
        if (keyword == null) return null;

        return new KeywordDto
        {
            Id = keyword.Id,
            Name = keyword.Name,
            CreatedAt = keyword.CreatedAt,
            ScenarioCount = keyword.ScenarioKeywords?.Count ?? 0
        };
    }

    public async Task<IEnumerable<KeywordDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var keywords = await _keywordRepository.SearchAsync(searchTerm, cancellationToken);
        return keywords.Select(k => new KeywordDto
        {
            Id = k.Id,
            Name = k.Name,
            CreatedAt = k.CreatedAt,
            ScenarioCount = k.ScenarioKeywords?.Count ?? 0
        });
    }

    public async Task<KeywordDto> CreateAsync(CreateKeywordRequest request, CancellationToken cancellationToken = default)
    {
        var keyword = new KnowledgeKeyword
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _keywordRepository.CreateAsync(keyword, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(Guid.Empty, AuditAction.KeywordAdded, "KnowledgeKeyword", created.Id, "Keyword Created", cancellationToken);

        return new KeywordDto
        {
            Id = created.Id,
            Name = created.Name,
            CreatedAt = created.CreatedAt,
            ScenarioCount = 0
        };
    }

    public async Task<KeywordDto> UpdateAsync(Guid id, CreateKeywordRequest request, CancellationToken cancellationToken = default)
    {
        var keyword = await _keywordRepository.GetByIdAsync(id, cancellationToken);
        if (keyword == null) throw new Exception("Keyword not found");

        keyword.Name = request.Name;
        await _keywordRepository.UpdateAsync(keyword, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new KeywordDto
        {
            Id = keyword.Id,
            Name = keyword.Name,
            CreatedAt = keyword.CreatedAt,
            ScenarioCount = keyword.ScenarioKeywords?.Count ?? 0
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _keywordRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(Guid.Empty, AuditAction.KeywordRemoved, "KnowledgeKeyword", id, "Keyword Deleted", cancellationToken);
    }
}
