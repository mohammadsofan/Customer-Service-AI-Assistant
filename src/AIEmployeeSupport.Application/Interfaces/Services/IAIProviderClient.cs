namespace AIEmployeeSupport.Application.Interfaces.Services;

/// <summary>
/// Abstraction for AI provider communication (OpenAI, Gemini, Anthropic, etc.).
/// Implementations handle the specific API calls for each provider.
/// </summary>
public interface IAIProviderClient
{
    Task<AIResponse> GenerateAnswerAsync(AIRequest request, CancellationToken cancellationToken = default);
}

public class AIRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int MaxTokens { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
}

public class AIResponse
{
    public bool Success { get; set; }
    public string Content { get; set; } = string.Empty;
    public int RequestTokens { get; set; }
    public int ResponseTokens { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}
