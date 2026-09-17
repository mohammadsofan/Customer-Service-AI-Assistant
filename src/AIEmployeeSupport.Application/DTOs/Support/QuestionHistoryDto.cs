namespace AIEmployeeSupport.Application.DTOs.Support;

public class QuestionHistoryDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeEmail { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool AnsweredByAI { get; set; }
    public double? ConfidenceScore { get; set; }
    public string? ProviderName { get; set; }
    public string? ModelName { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public long? ModelDurationMs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
