namespace AIEmployeeSupport.Application.DTOs.Support;

public class QuestionHistoryDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool AnsweredByAI { get; set; }
    public double? ConfidenceScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
