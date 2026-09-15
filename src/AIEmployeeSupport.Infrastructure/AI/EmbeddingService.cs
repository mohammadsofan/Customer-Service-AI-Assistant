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
            if (_settings.Provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(_settings.ApiKey))
            {
                var payload = new
                {
                    input = text,
                    model = _settings.ModelName
                };

                var response = await _httpClient.PostAsJsonAsync("embeddings", payload, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
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
                
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("OpenAI Embedding API error: {StatusCode} - {Error}. Falling back to deterministic embedding.", response.StatusCode, errorContent);
            }

            // Built-in deterministic semantic feature embedding for Mock/offline environments
            return GenerateDeterministicEmbedding(text, 256);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding via provider, using deterministic fallback.");
            return GenerateDeterministicEmbedding(text, 256);
        }
    }

    private static float[] GenerateDeterministicEmbedding(string text, int dimensions = 256)
    {
        var vector = new float[dimensions];
        if (string.IsNullOrWhiteSpace(text)) return vector;

        var normalized = NormalizeArabicText(text);

        var words = normalized.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return vector;

        using var sha = System.Security.Cryptography.SHA256.Create();
        foreach (var word in words)
        {
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(word));
            for (int i = 0; i < 4; i++)
            {
                var bucket = BitConverter.ToUInt16(hash, i * 2) % dimensions;
                var sign = (hash[8 + i] % 2 == 0) ? 1.0f : -1.0f;
                vector[bucket] += sign;
            }

            if (word.Length >= 3)
            {
                for (int j = 0; j <= word.Length - 3; j++)
                {
                    var trigram = word.Substring(j, 3);
                    var triHash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(trigram));
                    var bucket = BitConverter.ToUInt16(triHash, 0) % dimensions;
                    vector[bucket] += 0.5f;
                }
            }
        }

        double sumSq = 0;
        for (int i = 0; i < dimensions; i++) sumSq += vector[i] * vector[i];
        var norm = (float)Math.Sqrt(sumSq);
        if (norm > 0)
        {
            for (int i = 0; i < dimensions; i++) vector[i] /= norm;
        }

        return vector;
    }

    public static string NormalizeArabicText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.ToLowerInvariant();

        // 1. Strip Tashkeel (diacritics: Fatha, Damma, Kasra, Sukun, Tanwin, Shadda, etc.) & Tatweel
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[\u064B-\u065F\u0670\u0640]", "");

        // 2. Standard Arabic letter normalization
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[أإآء]", "ا");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "ة", "ه");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "ى", "ي");

        // 3. Conservative handling of attached preposition contraction 'ع الـ' / 'عالـ' -> 'على ال'
        // High precision: word starts with 'عال' followed by 2+ Arabic letters
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\bعال(?=[\p{L}]{2,}\b)", "على ال");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\bع\s+ال(?=[\p{L}]{2,}\b)", "على ال");

        // 4. Conservative pronoun enclitic detachment on action verbs (e.g. 'افحصله' -> 'افحص له')
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\b(?<verb>[\p{L}]{3,})(?<clitic>له|لها)\b", "${verb} ${clitic}");

        // 5. Clean punctuation / non-word characters
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^\w\s]", " ");

        return normalized;
    }
}
