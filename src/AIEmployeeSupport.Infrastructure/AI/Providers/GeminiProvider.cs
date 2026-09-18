using System.Net.Http.Json;
using System.Text.Json;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.Infrastructure.AI.Providers;

public class GeminiProvider : BaseAIProvider
{
    public GeminiProvider(HttpClient httpClient, string apiKey) : base(httpClient, apiKey)
    {
        HttpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/models/");
    }

    public override async Task<AIResponse> GenerateAnswerAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        string systemPrompt;
        if (request.ExpectsStandardEnvelope)
        {
            systemPrompt = $@"{request.SystemPrompt}
        
You must always respond in JSON format matching exactly this schema:
{{
    ""answered"": boolean,
    ""summary"": string or null,
    ""steps"": array of strings,
    ""reason"": string or null
}}

Retrieved Knowledge:
{string.Join("\n---\n", request.RetrievedKnowledge)}";
        }
        else
        {
            systemPrompt = $@"{request.SystemPrompt}

Retrieved Knowledge:
{string.Join("\n---\n", request.RetrievedKnowledge)}";
        }

        var payload = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = request.QuestionText } } }
            },
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            generationConfig = new
            {
                temperature = request.Temperature,
                maxOutputTokens = request.MaxTokens > 0 ? request.MaxTokens : 256,
                responseMimeType = "application/json"
            }
        };

        var response = await HttpClient.PostAsJsonAsync($"models/{request.ModelName}:generateContent?key={ApiKey}", payload, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

        HandleHttpError(response, rawContent);

        using var doc = JsonDocument.Parse(rawContent);
        var contentStr = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()?.Trim() ?? "{}";

        if (contentStr.StartsWith("```"))
        {
            var firstLineEnd = contentStr.IndexOf('\n');
            if (firstLineEnd != -1)
            {
                contentStr = contentStr.Substring(firstLineEnd + 1);
            }
            if (contentStr.EndsWith("```"))
            {
                contentStr = contentStr.Substring(0, contentStr.Length - 3);
            }
            contentStr = contentStr.Trim();
        }

        AIResponse aiResponse;
        if (request.ExpectsStandardEnvelope)
        {
            aiResponse = JsonSerializer.Deserialize<AIResponse>(contentStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new AIResponse();
        }
        else
        {
            aiResponse = new AIResponse { Answered = true, Summary = contentStr };
        }
            
        aiResponse.RawResponse = rawContent;
        return aiResponse;
    }

    public override async Task<string?> RewriteQueryAsync(string questionText, string modelName, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            system_instruction = new { parts = new[] { new { text = QueryRewriteSystemPrompt } } },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = $"<USER_INPUT>\n{questionText}\n</USER_INPUT>" } } }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 64,
                responseMimeType = "application/json"
            }
        };

        var url = $"./{modelName}:generateContent?key={ApiKey}";
        var response = await HttpClient.PostAsJsonAsync(url, payload, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

        HandleHttpError(response, rawContent);

        using var doc = JsonDocument.Parse(rawContent);
        var contentStr = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString()?.Trim() ?? string.Empty;

        return ExtractSearchQueryFromJson(contentStr);
    }

    public override async Task<float[]> GenerateEmbeddingAsync(string text, string modelName, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = $"models/{modelName}",
            content = new
            {
                parts = new[] { new { text = text } }
            }
        };

        var url = $"./{modelName}:embedContent?key={ApiKey}";
        var response = await HttpClient.PostAsJsonAsync(url, payload, cancellationToken);
        var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

        HandleHttpError(response, rawContent);

        using var doc = JsonDocument.Parse(rawContent);
        var dataArray = doc.RootElement.GetProperty("embedding").GetProperty("values");

        var floats = new float[dataArray.GetArrayLength()];
        var i = 0;
        foreach (var element in dataArray.EnumerateArray())
        {
            floats[i++] = element.GetSingle();
        }

        return floats;
    }
}
