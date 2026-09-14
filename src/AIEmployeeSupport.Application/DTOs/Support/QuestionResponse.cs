namespace AIEmployeeSupport.Application.DTOs.Support;

public class DetailedStepDto
{
    public int Order { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class QuestionResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Answered { get; set; }
    public string? Answer { get; set; }
    public List<string> Steps { get; set; } = new();
    public List<DetailedStepDto> DetailedSteps { get; set; } = new();
    public double? ConfidenceScore { get; set; }
    public string? SourceScenario { get; set; }
    public bool Escalated { get; set; }
}
