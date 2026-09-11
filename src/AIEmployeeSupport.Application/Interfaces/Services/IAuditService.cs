using AIEmployeeSupport.Application.DTOs.Audit;
using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(Guid userId, AuditAction action, string entityType, Guid? entityId = null, string? metadata = null, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<AuditLogDto>> GetLogsAsync(PaginatedRequest request, Guid? userId = null, AuditAction? action = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
}
