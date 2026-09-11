using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class AIRequestLogRepository : IAIRequestLogRepository
{
    private readonly ApplicationDbContext _context;

    public AIRequestLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(AIRequestLog requestLog, CancellationToken cancellationToken = default)
    {
        _context.AIRequestLogs.Add(requestLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<AIRequestLog>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default)
        => await _context.AIRequestLogs.AsNoTracking()
            .Where(r => r.QuestionId == questionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
}
