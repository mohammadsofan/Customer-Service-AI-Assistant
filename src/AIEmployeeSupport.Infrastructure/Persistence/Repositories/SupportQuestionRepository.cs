using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AIEmployeeSupport.Infrastructure.Persistence.Repositories;

public class SupportQuestionRepository : ISupportQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public SupportQuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupportQuestion> CreateAsync(SupportQuestion question, CancellationToken cancellationToken = default)
    {
        _context.SupportQuestions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);
        return question;
    }

    public async Task UpdateAsync(SupportQuestion question, CancellationToken cancellationToken = default)
    {
        _context.SupportQuestions.Update(question);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SupportQuestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.SupportQuestions.AsNoTracking()
            .Include(q => q.Employee)
            .Include(q => q.Scenario)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task<(IEnumerable<SupportQuestion> Items, int TotalCount)> GetByEmployeeIdAsync(
        Guid employeeId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.SupportQuestions.AsNoTracking()
            .Include(q => q.Employee)
            .Where(q => q.EmployeeId == employeeId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<(IEnumerable<SupportQuestion> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
        => GetAllAsync(page, pageSize, null, null, cancellationToken);

    public async Task<(IEnumerable<SupportQuestion> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, QuestionStatus? status, DateTime? date, CancellationToken cancellationToken = default)
    {
        var query = _context.SupportQuestions.AsNoTracking()
            .Include(q => q.Employee)
            .Include(q => q.Scenario)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(q => q.Status == status.Value);
        }

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            var nextDate = targetDate.AddDays(1);
            query = query.Where(q => q.CreatedAt >= targetDate && q.CreatedAt < nextDate);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<SupportQuestion>> GetUnansweredAsync(CancellationToken cancellationToken = default)
        => await _context.SupportQuestions.AsNoTracking()
            .Include(q => q.Employee)
            .Where(q => q.Status == QuestionStatus.New || q.Status == QuestionStatus.NoAnswer)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
}
