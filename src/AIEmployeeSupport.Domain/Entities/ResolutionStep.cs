namespace AIEmployeeSupport.Domain.Entities;

public class ResolutionStep
{
    public Guid Id { get; set; }
    public Guid ScenarioId { get; set; }
    public int StepOrder { get; set; }
    public string StepText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public KnowledgeScenario Scenario { get; set; } = null!;
}
