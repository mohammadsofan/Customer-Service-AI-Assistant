namespace AIEmployeeSupport.Domain.Entities;

public class AIRequestLog
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ModelId { get; set; }
    public int? RequestTokens { get; set; }
    public int? ResponseTokens { get; set; }
    public long DurationMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsFailover { get; set; }
    public DateTime CreatedAt { get; set; }
}
