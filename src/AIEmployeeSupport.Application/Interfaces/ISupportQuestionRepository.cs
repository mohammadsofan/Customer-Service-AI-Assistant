using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Interfaces;

public interface ISupportQuestionRepository
{
    Task<SupportQuestion> CreateAsync(SupportQuestion question, CancellationToken cancellationToken = default);
    Task UpdateAsync(SupportQuestion question, CancellationToken cancellationToken = default);
    Task<SupportQuestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<SupportQuestion> Items, int TotalCount)> GetByEmployeeIdAsync(
        Guid employeeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<(IEnumerable<SupportQuestion> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<(IEnumerable<SupportQuestion> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        Domain.Enums.QuestionStatus? status,
        DateTime? date,
        CancellationToken cancellationToken = default);
    Task<(IEnumerable<SupportQuestion> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        Domain.Enums.QuestionStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<SupportQuestion>> GetUnansweredAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}
