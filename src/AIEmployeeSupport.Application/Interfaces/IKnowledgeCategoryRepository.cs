using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IKnowledgeCategoryRepository
{
    Task<IEnumerable<KnowledgeCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<KnowledgeCategory> Items, int TotalCount)> GetAllAsync(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<KnowledgeCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeCategory> CreateAsync(KnowledgeCategory category, CancellationToken cancellationToken = default);
    Task UpdateAsync(KnowledgeCategory category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
