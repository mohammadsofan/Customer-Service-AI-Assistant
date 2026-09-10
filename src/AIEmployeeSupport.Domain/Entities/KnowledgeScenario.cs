using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Domain.Entities;

public class KnowledgeScenario
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public ScenarioStatus Status { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public KnowledgeCategory Category { get; set; } = null!;
    public ICollection<ResolutionStep> ResolutionSteps { get; set; } = new List<ResolutionStep>();
    public ICollection<ScenarioKeyword> ScenarioKeywords { get; set; } = new List<ScenarioKeyword>();
    public KnowledgeEmbedding? Embedding { get; set; }
    public ICollection<KnowledgeScenarioVersion> Versions { get; set; } = new List<KnowledgeScenarioVersion>();
}
