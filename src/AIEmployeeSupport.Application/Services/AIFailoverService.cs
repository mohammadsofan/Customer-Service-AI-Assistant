using System.Net;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Services;

public class AIFailoverService : IAIFailoverService
{
    private readonly IAIProviderRepository _providerRepository;
    private readonly IAIConfigurationRepository _configurationRepository;
    private readonly IAuditService _auditService;
    private readonly IAIProviderFactory _providerFactory;
    private readonly IEncryptionService _encryptionService;

    public AIFailoverService(
        IAIProviderRepository providerRepository,
        IAIConfigurationRepository configurationRepository,
        IAuditService auditService,
        IAIProviderFactory providerFactory,
        IEncryptionService encryptionService)
    {
        _providerRepository = providerRepository;
        _configurationRepository = configurationRepository;
        _auditService = auditService;
        _providerFactory = providerFactory;
        _encryptionService = encryptionService;
    }

    public async Task<AIResponse> GenerateAnswerWithFailoverAsync(AIRequest request, CancellationToken token = default)
    {
        var config = await _configurationRepository.GetAsync(token);
        if (config == null) throw new InvalidOperationException("AI configuration not found.");

        var providers = (await _providerRepository.GetActiveAsync(token)).OrderBy(p => p.FallbackPriority).ToList();
        if (!providers.Any()) throw new InvalidOperationException("No active AI providers found.");

        // Move active provider to front if possible, or just follow priority
        var primaryProvider = providers.FirstOrDefault(p => p.Id == config.ActiveProviderId) ?? providers.First();

        var triedProviders = new HashSet<Guid>();
        
        Exception lastException = null!;

        foreach (var provider in providers)
        {
            if (triedProviders.Contains(provider.Id)) continue;
            triedProviders.Add(provider.Id);

            var apiKey = _encryptionService.Decrypt(provider.EncryptedApiKey);
            var client = _providerFactory.CreateClient(provider.ProviderType, apiKey);

            try
            {
                var response = await client.GenerateAnswerAsync(request, token);
                return response;
            }
            catch (Exception ex) when (IsRateLimitOrQuotaError(ex))
            {
                lastException = ex;
                if (!config.EnableAutoFailover)
                {
                    throw; // Auto failover is disabled
                }

                await _auditService.LogAsync(Guid.Empty, AuditAction.AIProviderFailoverTriggered, "AIProvider", provider.Id, $"Failed with {ex.Message}. Falling back to next priority.", token);
            }
        }

        throw new InvalidOperationException("All AI providers failed.", lastException);
    }

    private bool IsRateLimitOrQuotaError(Exception ex)
    {
        if (ex is HttpRequestException httpEx && httpEx.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return true;
        }

        var msg = ex.Message.ToLowerInvariant();
        return msg.Contains("429") || msg.Contains("rate limit") || msg.Contains("quota") || msg.Contains("too many requests");
    }
}
