using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class KnowledgeCategoryRepository : IKnowledgeCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public KnowledgeCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KnowledgeCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.KnowledgeCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task<KnowledgeCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.KnowledgeCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<KnowledgeCategory> CreateAsync(KnowledgeCategory category, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task UpdateAsync(KnowledgeCategory category, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeCategories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.KnowledgeCategories.FindAsync(new object[] { id }, cancellationToken);
        if (category != null)
        {
            _context.KnowledgeCategories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
