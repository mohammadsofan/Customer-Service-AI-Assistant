namespace AIEmployeeSupport.Application.Interfaces.Services;

/// <summary>
/// Abstraction for AI provider communication (OpenAI, Gemini, Anthropic, etc.).
/// Implementations handle the specific API calls for each provider.
/// </summary>
public interface IAIProviderClient
{
    Task<AIResponse> GenerateAnswerAsync(AIRequest request, CancellationToken cancellationToken = default);
    Task<string?> RewriteQueryAsync(string questionText, string modelName, CancellationToken cancellationToken = default);
    Task<float[]> GenerateEmbeddingAsync(string text, string modelName, CancellationToken cancellationToken = default);
}
