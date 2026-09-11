using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class KnowledgeScenarioVersionRepository : IKnowledgeScenarioVersionRepository
{
    private readonly ApplicationDbContext _context;

    public KnowledgeScenarioVersionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KnowledgeScenarioVersion>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken cancellationToken = default)
        => await _context.KnowledgeScenarioVersions.AsNoTracking()
            .Where(v => v.ScenarioId == scenarioId)
            .OrderByDescending(v => v.Version)
            .ToListAsync(cancellationToken);

    public async Task<KnowledgeScenarioVersion> CreateAsync(KnowledgeScenarioVersion version, CancellationToken cancellationToken = default)
    {
        _context.KnowledgeScenarioVersions.Add(version);
        await _context.SaveChangesAsync(cancellationToken);
        return version;
    }
}
