using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Interfaces;

public interface IAuditLogRepository
{
    Task CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        Guid? userId = null,
        AuditAction? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}
