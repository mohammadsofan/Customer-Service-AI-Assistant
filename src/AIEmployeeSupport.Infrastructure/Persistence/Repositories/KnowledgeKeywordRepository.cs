using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class KnowledgeKeywordRepository : IKnowledgeKeywordRepository
{
    private readonly ApplicationDbContext _context;

    public KnowledgeKeywordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KnowledgeKeyword>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.KnowledgeKeywords.AsNoTracking().OrderBy(k => k.Name).ToListAsync(cancellationToken);

    public async Task<(IEnumerable<KnowledgeKeyword> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var query = _context.KnowledgeKeywords.AsNoTracking().Include(k => k.ScenarioKeywords).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(k => k.Name.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(k => k.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<KnowledgeKeyword?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.KnowledgeKeywords.AsNoTracking().FirstOrDefaultAsync(k => k.Id == id, cancellationToken);

    public async Task<IEnumerable<KnowledgeKeyword>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        => await _context.KnowledgeKeywords.AsNoTracking()
            .Where(k => k.Name.Contains(searchTerm))
            .OrderBy(k => k.Name)
            .ToListAsync(cancellationToken);

    public async Task<KnowledgeKeyword> CreateAsync(KnowledgeKeyword keyword, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeKeywords.Add(keyword);
        await _context.SaveChangesAsync(cancellationToken);
        return keyword;
    }

    public async Task UpdateAsync(KnowledgeKeyword keyword, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeKeywords.Update(keyword);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var keyword = await _context.KnowledgeKeywords.FindAsync(new object[] { id }, cancellationToken);
        if (keyword != null)
        {
            _context.KnowledgeKeywords.Remove(keyword);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
