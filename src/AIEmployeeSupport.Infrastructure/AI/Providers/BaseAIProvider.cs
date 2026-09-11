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

        // Check if Cloudflare or similar WAF blocked the request
        if (rawContent.Contains("Attention Required! | Cloudflare", StringComparison.OrdinalIgnoreCase) ||
            (rawContent.Contains("Cloudflare", StringComparison.OrdinalIgnoreCase) && rawContent.Contains("blocked", StringComparison.OrdinalIgnoreCase)))
        {
            throw new AIProviderException(AIErrorCategory.AuthenticationError,
                $"Cloudflare WAF Blocked (HTTP {statusCode}): The target AI host blocked requests from this server IP. Check Cloudflare WAF/Firewall rules or use an unblocked endpoint.",
                rawContent);
        }

        var errorDetail = ExtractErrorMessage(rawContent);
        
        // 429 = Rate Limit or Quota Exceeded (common for OpenAI/Anthropic)
        if (statusCode == 429)
        {
            if (rawContent.Contains("quota", StringComparison.OrdinalIgnoreCase) || 
                rawContent.Contains("insufficient_quota", StringComparison.OrdinalIgnoreCase))
            {
                throw new AIProviderException(AIErrorCategory.QuotaExceededError, $"Quota exceeded: {errorDetail}", rawContent);
            }
            throw new AIProviderException(AIErrorCategory.RateLimitError, $"Rate limit hit: {errorDetail}", rawContent);
        }
        
        // 401/403 = Auth Error
        if (statusCode == 401 || statusCode == 403)
        {
            throw new AIProviderException(AIErrorCategory.AuthenticationError, $"Authentication failed (HTTP {statusCode}): {errorDetail}", rawContent);
        }

        // Catch-all
        throw new AIProviderException(AIErrorCategory.GeneralError, $"API Error ({statusCode}): {errorDetail}", rawContent);
    }

    private static string ExtractErrorMessage(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
            return "No response details provided by remote server.";

        try
        {
            using var doc = JsonDocument.Parse(rawContent);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var errorEl))
            {
                if (errorEl.ValueKind == JsonValueKind.Object && errorEl.TryGetProperty("message", out var msgEl))
                    return msgEl.GetString() ?? errorEl.ToString();
                return errorEl.ToString();
            }
            if (root.TryGetProperty("message", out var directMsg))
                return directMsg.GetString() ?? rawContent;
        }
        catch
        {
            // Not JSON
        }

        if (rawContent.TrimStart().StartsWith("<", StringComparison.OrdinalIgnoreCase))
        {
            var match = System.Text.RegularExpressions.Regex.Match(rawContent, @"<title>(.*?)</title>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return $"HTML Error: {match.Groups[1].Value.Trim()}";
            }
            return "Remote server returned an HTML error page.";
        }

        return rawContent.Length > 200 ? rawContent.Substring(0, 200) + "..." : rawContent;
    }
}
