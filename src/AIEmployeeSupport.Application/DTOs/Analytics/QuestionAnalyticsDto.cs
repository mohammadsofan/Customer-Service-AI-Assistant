namespace AIEmployeeSupport.Application.DTOs.Analytics;

public class QuestionAnalyticsDto
{
    public List<QuestionAnalyticsItem> Questions { get; set; } = new();
}

public class QuestionAnalyticsItem
{
    public Guid Id { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeEmail { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool AnsweredByAI { get; set; }
    public double? ConfidenceScore { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public string? ScenarioName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
