using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Domain.Entities;

public class KnowledgeEmbedding
{
    public Guid Id { get; set; }
    public Guid ScenarioId { get; set; }
    public string Content { get; set; } = string.Empty;
    public byte[] Embedding { get; set; } = Array.Empty<byte>();
    public EmbeddingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public KnowledgeScenario Scenario { get; set; } = null!;
}
