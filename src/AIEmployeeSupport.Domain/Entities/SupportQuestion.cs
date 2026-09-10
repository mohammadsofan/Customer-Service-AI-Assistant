using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Domain.Entities;

public class SupportQuestion
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionStatus Status { get; set; }
    public string? AnswerText { get; set; }
    public bool AnsweredByAI { get; set; }
    public bool Escalated { get; set; }
    public double? ConfidenceScore { get; set; }
    public Guid? ScenarioId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? ProcessingTimeMs { get; set; }

    // Navigation properties
    public User Employee { get; set; } = null!;
    public KnowledgeScenario? Scenario { get; set; }
}
