namespace AIEmployeeSupport.Application.DTOs.Audit;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Metadata { get; set; }
}
