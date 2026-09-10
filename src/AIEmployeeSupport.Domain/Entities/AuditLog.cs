using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Metadata { get; set; }
}
