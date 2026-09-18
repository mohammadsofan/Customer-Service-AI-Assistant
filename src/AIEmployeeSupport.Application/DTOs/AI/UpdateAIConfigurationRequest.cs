namespace AIEmployeeSupport.Application.DTOs.AI;

public class UpdateAIConfigurationRequest
{
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
    public bool LLMRerankingEnabled { get; set; } = true;
    public int LLMRerankingTopK { get; set; } = 3;
    public double LLMRerankingConfidenceThreshold { get; set; } = 0.5;
}
