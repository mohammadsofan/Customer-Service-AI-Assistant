using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class KnowledgeScenarioRepository : IKnowledgeScenarioRepository
{
    private readonly ApplicationDbContext _context;

    public KnowledgeScenarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<KnowledgeScenario> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, ScenarioStatus? status = null, Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.KnowledgeScenarios
            .AsNoTracking()
            .Include(s => s.Category)
            .Include(s => s.ScenarioKeywords)
            .Include(s => s.ResolutionSteps)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);
        if (categoryId.HasValue)
            query = query.Where(s => s.CategoryId == categoryId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<KnowledgeScenario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.KnowledgeScenarios
            .AsNoTracking()
            .Include(s => s.Category)
            .Include(s => s.ResolutionSteps.OrderBy(r => r.StepOrder))
            .Include(s => s.ScenarioKeywords).ThenInclude(sk => sk.Keyword)
            .Include(s => s.Embedding)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IEnumerable<KnowledgeScenario>> GetByStatusAsync(ScenarioStatus status, CancellationToken cancellationToken = default)
        => await _context.KnowledgeScenarios.AsNoTracking()
            .Where(s => s.Status == status)
            .Include(s => s.Category)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<KnowledgeScenario>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
        => await _context.KnowledgeScenarios.AsNoTracking()
            .Where(s => s.Name.Contains(searchTerm) || s.Description.Contains(searchTerm))
            .Include(s => s.Category)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<KnowledgeScenario> CreateAsync(KnowledgeScenario scenario, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeScenarios.Add(scenario);
        await _context.SaveChangesAsync(cancellationToken);
        return scenario;
    }

    public async Task UpdateAsync(KnowledgeScenario scenario, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeScenarios.Update(scenario);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var scenario = await _context.KnowledgeScenarios.FindAsync(new object[] { id }, cancellationToken);
        if (scenario != null)
        {
            _context.KnowledgeScenarios.Remove(scenario);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
