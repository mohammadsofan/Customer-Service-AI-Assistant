namespace AIEmployeeSupport.Domain.Entities;

public class KnowledgeCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<KnowledgeScenario> Scenarios { get; set; } = new List<KnowledgeScenario>();
}
