using AIEmployeeSupport.Application.DTOs.Knowledge;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IKeywordService
{
    Task<IEnumerable<KeywordDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<KeywordDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<KeywordDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<KeywordDto> CreateAsync(CreateKeywordRequest request, CancellationToken cancellationToken = default);
    Task<KeywordDto> UpdateAsync(Guid id, CreateKeywordRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
