namespace AIEmployeeSupport.Domain.Entities;

public class KnowledgeScenarioVersion
{
    public Guid Id { get; set; }
    public Guid ScenarioId { get; set; }
    public int Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ResolutionStepsSnapshot { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public KnowledgeScenario Scenario { get; set; } = null!;
}
