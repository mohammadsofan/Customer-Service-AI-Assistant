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

        var providers = (await _providerRepository.GetActiveAsync(token)).ToList();
        if (!providers.Any()) throw new InvalidOperationException("No active AI providers found.");

        // Move active provider to front, then order remaining by FallbackPriority
        var orderedProviders = providers
            .OrderBy(p => p.Id == config.ActiveProviderId ? 0 : 1)
            .ThenBy(p => p.FallbackPriority)
            .ToList();

        var triedProviders = new HashSet<Guid>();
        Exception lastException = null!;

        foreach (var provider in orderedProviders)
        {
            if (triedProviders.Contains(provider.Id)) continue;
            triedProviders.Add(provider.Id);

            string apiKey;
            try
            {
                apiKey = _encryptionService.Decrypt(provider.EncryptedApiKey);
            }
            catch
            {
                apiKey = provider.EncryptedApiKey;
            }

            var client = _providerFactory.CreateClient(provider.ProviderType, apiKey, provider.BaseUrl);

            var modelToUse = (provider.Id == config.ActiveProviderId && config.ActiveModel != null)
                ? config.ActiveModel.ModelName
                : (provider.Models?.FirstOrDefault(m => m.IsActive)?.ModelName ?? request.ModelName);

            var reqForProvider = new AIRequest
            {
                QuestionText = request.QuestionText,
                RetrievedKnowledge = request.RetrievedKnowledge,
                SystemPrompt = request.SystemPrompt,
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                ModelName = modelToUse
            };

            try
            {
                var response = await client.GenerateAnswerAsync(reqForProvider, token);
                return response;
            }
            catch (Exception ex)
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
}
