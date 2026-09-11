using System.Net.Http;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;
using AIEmployeeSupport.Infrastructure.AI.Providers;

namespace AIEmployeeSupport.Infrastructure.AI;

public class AIProviderFactory : IAIProviderFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AIProviderFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IAIProviderClient CreateClient(ProviderType providerType, string apiKey, string? baseUrl = null)
    {
        var httpClient = _httpClientFactory.CreateClient("AIProviderClient");

        return providerType switch
        {
            ProviderType.OpenAI => new OpenAIProvider(httpClient, apiKey, baseUrl),
            ProviderType.Gemini => new GeminiProvider(httpClient, apiKey),
            ProviderType.Anthropic => new AnthropicProvider(httpClient, apiKey),
            // Custom or AzureOpenAI can be expanded here. Defaulting to OpenAI style for Azure/Custom
            ProviderType.AzureOpenAI => new OpenAIProvider(httpClient, apiKey, baseUrl),
            ProviderType.Custom => new OpenAIProvider(httpClient, apiKey, baseUrl),
            _ => throw new NotSupportedException($"Provider type {providerType} is not supported.")
        };
    }
}
