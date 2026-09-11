using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.Infrastructure.AI.Providers;

public class OpenAIProvider : BaseAIProvider
{
    public OpenAIProvider(HttpClient httpClient, string apiKey, string? baseUrl = null) : base(httpClient, apiKey)
    {
        var targetBaseUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? "https://api.openai.com/v1/"
            : baseUrl.TrimEnd('/') + "/";

        HttpClient.BaseAddress = new Uri(targetBaseUrl);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
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
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = request.QuestionText }
            },
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            response_format = new { type = "json_object" }
        };

        var response = await HttpClient.PostAsJsonAsync("chat/completions", payload, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

        HandleHttpError(response, rawContent);

        using var doc = JsonDocument.Parse(rawContent);
        var contentStr = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        
        var aiResponse = JsonSerializer.Deserialize<AIResponse>(contentStr ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new AIResponse();
            
        aiResponse.RawResponse = rawContent;
        return aiResponse;
    }
}
