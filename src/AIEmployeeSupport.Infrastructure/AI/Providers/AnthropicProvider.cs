using System.Net.Http.Json;
using System.Text.Json;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.Infrastructure.AI.Providers;

public class AnthropicProvider : BaseAIProvider
{
    public AnthropicProvider(HttpClient httpClient, string apiKey) : base(httpClient, apiKey)
    {
        HttpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
        HttpClient.DefaultRequestHeaders.Add("x-api-key", ApiKey);
        HttpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public override async Task<AIResponse> GenerateAnswerAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var systemPrompt = $@"{request.SystemPrompt}
        
You must always respond in JSON format matching exactly this schema:
{{
    ""answered"": boolean,
    ""summary"": string or null,
    ""steps"": array of strings,
    ""reason"": string or null
}}

Retrieved Knowledge:
{string.Join("\n---\n", request.RetrievedKnowledge)}";

        var payload = new
        {
            model = request.ModelName,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = request.QuestionText }
            },
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };

        var response = await HttpClient.PostAsJsonAsync("messages", payload, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

        HandleHttpError(response, rawContent);

        using var doc = JsonDocument.Parse(rawContent);
        var contentStr = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
        
        // Anthropic doesn't have strict JSON mode without tool use, so we extract JSON blocks if present
        if (contentStr != null && contentStr.Contains("```json"))
        {
            var start = contentStr.IndexOf("```json") + 7;
            var end = contentStr.IndexOf("```", start);
            if (end > start)
            {
                contentStr = contentStr.Substring(start, end - start).Trim();
            }
        }

        var aiResponse = JsonSerializer.Deserialize<AIResponse>(contentStr ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new AIResponse();
            
        aiResponse.RawResponse = rawContent;
        return aiResponse;
    }

    public override async Task<string?> RewriteQueryAsync(string questionText, string modelName, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = modelName,
            system = QueryRewriteSystemPrompt,
            messages = new[]
            {
                new { role = "user", content = $"<USER_INPUT>\n{questionText}\n</USER_INPUT>" }
            },
            temperature = 0.1,
            max_tokens = 64
        };

        var response = await HttpClient.PostAsJsonAsync("messages", payload, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

        HandleHttpError(response, rawContent);

        using var doc = JsonDocument.Parse(rawContent);
        var contentStr = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString()?.Trim() ?? string.Empty;

        return ExtractSearchQueryFromJson(contentStr);
    }
}
