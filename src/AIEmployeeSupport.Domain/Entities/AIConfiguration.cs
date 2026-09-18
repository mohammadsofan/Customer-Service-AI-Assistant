namespace AIEmployeeSupport.Domain.Entities;

public class AIConfiguration
{
    public Guid Id { get; set; }
    public Guid ActiveProviderId { get; set; }
    public Guid ActiveModelId { get; set; }
    public Guid? ActiveEmbeddingProviderId { get; set; }
    public Guid? ActiveEmbeddingModelId { get; set; }
    public double Temperature { get; set; }
    public int MaxTokens { get; set; }
    public double SimilarityThreshold { get; set; }
    public int TopK { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public bool EnableAutoFailover { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public AIProvider ActiveProvider { get; set; } = null!;
    public AIModel ActiveModel { get; set; } = null!;
    public AIProvider? ActiveEmbeddingProvider { get; set; }
    public AIModel? ActiveEmbeddingModel { get; set; }
}
