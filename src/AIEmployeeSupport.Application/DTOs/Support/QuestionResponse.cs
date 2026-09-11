namespace AIEmployeeSupport.Application.DTOs.Support;

public class QuestionResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Answered { get; set; }
    public string? Answer { get; set; }
    public List<string> Steps { get; set; } = new();
    public double? ConfidenceScore { get; set; }
    public string? SourceScenario { get; set; }
    public bool Escalated { get; set; }
}
