using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AIEmployeeSupport.Application.Common.Settings;
using AIEmployeeSupport.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIEmployeeSupport.Infrastructure.AI;

public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly EmbeddingSettings _settings;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        IHttpClientFactory httpClientFactory,
        IOptions<EmbeddingSettings> settings,
        ILogger<EmbeddingService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("EmbeddingClient");
        _settings = settings.Value;
        _logger = logger;

        if (_settings.Provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
        {
            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            if (!string.IsNullOrEmpty(_settings.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            }
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<float>();
        }

        try
        {
            if (_settings.Provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                var payload = new
                {
                    input = text,
                    model = _settings.ModelName
                };

                var response = await _httpClient.PostAsJsonAsync("embeddings", payload, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("OpenAI Embedding API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return Array.Empty<float>();
                }

                var resultStr = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(resultStr);
                
                var dataArray = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");
                
                var floats = new float[dataArray.GetArrayLength()];
                var i = 0;
                foreach (var element in dataArray.EnumerateArray())
                {
                    floats[i++] = element.GetSingle();
                }
                
                return floats;
            }

            _logger.LogWarning("Embedding provider '{Provider}' is not supported.", _settings.Provider);
            return Array.Empty<float>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding for text: {TextPreview}", text.Length > 50 ? text[..50] + "..." : text);
            return Array.Empty<float>();
        }
    }
}
