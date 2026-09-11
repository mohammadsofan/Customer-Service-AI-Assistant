using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class AIConfigurationRepository : IAIConfigurationRepository
{
    private readonly ApplicationDbContext _context;

    public AIConfigurationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AIConfiguration?> GetAsync(CancellationToken cancellationToken = default)
        => await _context.AIConfigurations.AsNoTracking()
            .Include(c => c.ActiveProvider)
            .Include(c => c.ActiveModel)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task UpdateAsync(AIConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _context.AIConfigurations.Update(configuration);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
