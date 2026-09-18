namespace AIEmployeeSupport.Application.DTOs.AI;

public class AIConfigurationDto
{
    public Guid ActiveProviderId { get; set; }
    public string ActiveProviderName { get; set; } = string.Empty;
    public Guid ActiveModelId { get; set; }
    public string ActiveModelName { get; set; } = string.Empty;
    public Guid? ActiveEmbeddingProviderId { get; set; }
    public string? ActiveEmbeddingProviderName { get; set; }
    public Guid? ActiveEmbeddingModelId { get; set; }
    public string? ActiveEmbeddingModelName { get; set; }
    public double Temperature { get; set; }
    public int MaxTokens { get; set; }
    public double SimilarityThreshold { get; set; }
    public int TopK { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public bool EnableAutoFailover { get; set; }
    public bool LLMRerankingEnabled { get; set; }
    public int LLMRerankingTopK { get; set; }
    public double LLMRerankingConfidenceThreshold { get; set; }
}
