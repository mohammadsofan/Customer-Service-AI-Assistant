using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IKnowledgeKeywordRepository
{
    Task<IEnumerable<KnowledgeKeyword>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<KnowledgeKeyword> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<KnowledgeKeyword?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<KnowledgeKeyword>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<KnowledgeKeyword> CreateAsync(KnowledgeKeyword keyword, CancellationToken cancellationToken = default);
    Task UpdateAsync(KnowledgeKeyword keyword, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
