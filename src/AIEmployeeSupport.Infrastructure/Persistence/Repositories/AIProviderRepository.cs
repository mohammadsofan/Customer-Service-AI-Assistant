using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class AIProviderRepository : IAIProviderRepository
{
    private readonly ApplicationDbContext _context;

    public AIProviderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AIProvider>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.AIProviders.AsNoTracking()
            .Include(p => p.Models)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<AIProvider?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AIProviders.AsNoTracking()
            .Include(p => p.Models)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IEnumerable<AIProvider>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.AIProviders.AsNoTracking()
            .Where(p => p.IsActive)
            .Include(p => p.Models)
            .OrderBy(p => p.FallbackPriority)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<AIProvider>> GetByPriorityAsync(CancellationToken cancellationToken = default)
        => await _context.AIProviders.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.FallbackPriority)
            .ToListAsync(cancellationToken);

    public async Task<AIProvider> CreateAsync(AIProvider provider, CancellationToken cancellationToken = default)
    {
        _context.AIProviders.Add(provider);
        await _context.SaveChangesAsync(cancellationToken);
        return provider;
    }

    public async Task UpdateAsync(AIProvider provider, CancellationToken cancellationToken = default)
    {
        _context.AIProviders.Update(provider);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _context.AIProviders.FindAsync(new object[] { id }, cancellationToken);
        if (provider != null)
        {
            _context.AIProviders.Remove(provider);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
