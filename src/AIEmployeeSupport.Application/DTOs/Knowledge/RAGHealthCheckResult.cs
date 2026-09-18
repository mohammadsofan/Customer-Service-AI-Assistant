namespace AIEmployeeSupport.Application.DTOs.Knowledge;

public class RAGHealthCheckResult
{
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StoredEmbeddingsCount { get; set; }
    public int StoredDimension { get; set; }
    public int QueryDimension { get; set; }
    public string ActiveEmbeddingModel { get; set; } = string.Empty;
}
