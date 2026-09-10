namespace AIEmployeeSupport.Domain.Entities;

public class ScenarioKeyword
{
    public Guid ScenarioId { get; set; }
    public Guid KeywordId { get; set; }

    // Navigation properties
    public KnowledgeScenario Scenario { get; set; } = null!;
    public KnowledgeKeyword Keyword { get; set; } = null!;
}
