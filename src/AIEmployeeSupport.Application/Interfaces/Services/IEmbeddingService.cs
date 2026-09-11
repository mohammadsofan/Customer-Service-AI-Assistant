namespace AIEmployeeSupport.Application.Interfaces.Services;

/// <summary>
/// Service for generating text embeddings (vector representations).
/// Used for both question embedding and scenario indexing.
/// </summary>
public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
