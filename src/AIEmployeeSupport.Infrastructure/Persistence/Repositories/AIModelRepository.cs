using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class AIModelRepository : IAIModelRepository
{
    private readonly ApplicationDbContext _context;

    public AIModelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AIModel>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.AIModels.AsNoTracking()
            .Include(m => m.Provider)
            .OrderBy(m => m.ModelName)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<AIModel>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
        => await _context.AIModels.AsNoTracking()
            .Where(m => m.ProviderId == providerId)
            .OrderBy(m => m.ModelName)
            .ToListAsync(cancellationToken);

    public async Task<AIModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AIModels.AsNoTracking()
            .Include(m => m.Provider)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<AIModel> CreateAsync(AIModel model, CancellationToken cancellationToken = default)
    {
        _context.AIModels.Add(model);
        await _context.SaveChangesAsync(cancellationToken);
        return model;
    }

    public async Task UpdateAsync(AIModel model, CancellationToken cancellationToken = default)
    {
        _context.AIModels.Update(model);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.AIModels.FindAsync(new object[] { id }, cancellationToken);
        if (model != null)
        {
            _context.AIModels.Remove(model);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
