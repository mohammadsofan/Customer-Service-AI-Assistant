using AIEmployeeSupport.Application.DTOs.Knowledge;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IKnowledgeCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public CategoryService(IKnowledgeCategoryRepository categoryRepository, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            ScenarioCount = c.Scenarios?.Count ?? 0
        });
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ScenarioCount = category.Scenarios?.Count ?? 0
        };
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = new KnowledgeCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _categoryRepository.CreateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(Guid.Empty, AuditAction.CategoryChanged, "KnowledgeCategory", created.Id, "Category Created", cancellationToken);

        return new CategoryDto
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description,
            IsActive = created.IsActive,
            ScenarioCount = 0
        };
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null) throw new Exception("Category not found");

        category.Name = request.Name;
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(Guid.Empty, AuditAction.CategoryChanged, "KnowledgeCategory", category.Id, "Category Updated", cancellationToken);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            ScenarioCount = category.Scenarios?.Count ?? 0
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _categoryRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync(Guid.Empty, AuditAction.CategoryChanged, "KnowledgeCategory", id, "Category Deleted", cancellationToken);
    }
}
