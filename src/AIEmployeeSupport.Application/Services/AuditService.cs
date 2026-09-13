using AIEmployeeSupport.Application.DTOs.Audit;
using AIEmployeeSupport.Application.DTOs.Common;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(
        IAuditLogRepository auditLogRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(Guid userId, AuditAction action, string entityType, Guid? entityId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata
        };

        await _auditLogRepository.CreateAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<AuditLogDto>> GetLogsAsync(
        PaginatedRequest request,
        Guid? userId = null,
        AuditAction? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _auditLogRepository.GetAllAsync(
            request.Page,
            request.PageSize,
            userId,
            action,
            fromDate,
            toDate,
            cancellationToken);

        var auditLogDtos = new List<AuditLogDto>();
        
        foreach (var item in items)
        {
            var user = item.UserId != Guid.Empty ? await _userRepository.GetByIdAsync(item.UserId, cancellationToken) : null;
            var userName = user != null ? user.FullName : (item.UserId == Guid.Empty ? "النظام" : "مستخدم غير معروف");
            var userEmail = user?.Email;
            var userRole = user != null ? (user.Role == UserRole.Administrator ? "مدير النظام" : "موظف دعم") : (item.UserId == Guid.Empty ? "النظام" : null);

            auditLogDtos.Add(new AuditLogDto
            {
                Id = item.Id,
                UserId = item.UserId,
                UserName = userName,
                UserEmail = userEmail,
                UserRole = userRole,
                Action = item.Action.ToString(),
                EntityType = item.EntityType,
                EntityId = item.EntityId,
                Timestamp = item.Timestamp,
                Metadata = item.Metadata
            });
        }

        return new PaginatedResponse<AuditLogDto>
        {
            Items = auditLogDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
