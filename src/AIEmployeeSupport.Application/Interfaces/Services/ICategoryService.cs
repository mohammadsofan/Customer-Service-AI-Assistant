using AIEmployeeSupport.Application.DTOs.Knowledge;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
