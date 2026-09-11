using AIEmployeeSupport.Application.Exceptions;
using AIEmployeeSupport.Application.Interfaces.Services;
using System.Text.Json;

namespace AIEmployeeSupport.Infrastructure.AI.Providers;

public abstract class BaseAIProvider : IAIProviderClient
{
    protected readonly HttpClient HttpClient;
    protected readonly string ApiKey;

    protected BaseAIProvider(HttpClient httpClient, string apiKey)
    {
        HttpClient = httpClient;
        ApiKey = apiKey;
    }

    public abstract Task<AIResponse> GenerateAnswerAsync(AIRequest request, CancellationToken cancellationToken = default);

    protected void HandleHttpError(HttpResponseMessage response, string rawContent)
    {
        if (response.IsSuccessStatusCode) return;

        var statusCode = (int)response.StatusCode;
        
        // 429 = Rate Limit or Quota Exceeded (common for OpenAI/Anthropic)
        if (statusCode == 429)
        {
            if (rawContent.Contains("quota", StringComparison.OrdinalIgnoreCase) || 
                rawContent.Contains("insufficient_quota", StringComparison.OrdinalIgnoreCase))
            {
                throw new AIProviderException(AIErrorCategory.QuotaExceededError, $"Quota exceeded: {rawContent}", rawContent);
            }
            throw new AIProviderException(AIErrorCategory.RateLimitError, $"Rate limit hit: {rawContent}", rawContent);
        }
        
        // 401/403 = Auth Error
        if (statusCode == 401 || statusCode == 403)
        {
            throw new AIProviderException(AIErrorCategory.AuthenticationError, $"Authentication failed: {rawContent}", rawContent);
        }

        // Catch-all
        throw new AIProviderException(AIErrorCategory.GeneralError, $"API Error ({statusCode}): {rawContent}", rawContent);
    }
}
