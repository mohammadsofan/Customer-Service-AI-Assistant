using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class ResolutionStepRepository : IResolutionStepRepository
{
    private readonly ApplicationDbContext _context;

    public ResolutionStepRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ResolutionStep>> GetByScenarioIdAsync(Guid scenarioId, CancellationToken cancellationToken = default)
        => await _context.ResolutionSteps.AsNoTracking()
            .Where(r => r.ScenarioId == scenarioId)
            .OrderBy(r => r.StepOrder)
            .ToListAsync(cancellationToken);

    public async Task<ResolutionStep> CreateAsync(ResolutionStep step, CancellationToken cancellationToken = default)
    {
        _context.ResolutionSteps.Add(step);
        await _context.SaveChangesAsync(cancellationToken);
        return step;
    }

    public async Task UpdateAsync(ResolutionStep step, CancellationToken cancellationToken = default)
    {
        _context.ResolutionSteps.Update(step);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var step = await _context.ResolutionSteps.FindAsync(new object[] { id }, cancellationToken);
        if (step != null)
        {
            _context.ResolutionSteps.Remove(step);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ReorderStepsAsync(Guid scenarioId, IEnumerable<Guid> orderedStepIds, CancellationToken cancellationToken = default)
    {
        var steps = await _context.ResolutionSteps
            .Where(r => r.ScenarioId == scenarioId)
            .ToListAsync(cancellationToken);

        var order = 1;
        foreach (var stepId in orderedStepIds)
        {
            var step = steps.FirstOrDefault(s => s.Id == stepId);
            if (step != null)
            {
                step.StepOrder = order++;
                step.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
