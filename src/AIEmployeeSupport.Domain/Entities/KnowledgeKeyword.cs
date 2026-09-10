namespace AIEmployeeSupport.Domain.Entities;

public class KnowledgeKeyword
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<ScenarioKeyword> ScenarioKeywords { get; set; } = new List<ScenarioKeyword>();
}
