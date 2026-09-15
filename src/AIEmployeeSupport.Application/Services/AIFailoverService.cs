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

            // Determine models to try for this provider
            var candidateModels = new List<string>();

            if (provider.Id == config.ActiveProviderId && config.ActiveModel != null)
            {
                // Primary active model first
                candidateModels.Add(config.ActiveModel.ModelName);

                // Followed by other active models for this provider
                if (provider.Models != null)
                {
                    var fallbackModels = provider.Models
                        .Where(m => m.IsActive && m.Id != config.ActiveModelId)
                        .Select(m => m.ModelName)
                        .Distinct();
                    candidateModels.AddRange(fallbackModels);
                }
            }
            else if (provider.Models != null && provider.Models.Any(m => m.IsActive))
            {
                candidateModels.AddRange(provider.Models.Where(m => m.IsActive).Select(m => m.ModelName).Distinct());
            }

            if (!candidateModels.Any())
            {
                candidateModels.Add(request.ModelName);
            }

            foreach (var modelToUse in candidateModels)
            {
                var reqForModel = new AIRequest
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
                    var response = await client.GenerateAnswerAsync(reqForModel, token);
                    return response;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (!config.EnableAutoFailover)
                    {
                        throw; // Auto failover is disabled
                    }

                    await _auditService.LogAsync(
                        Guid.Empty,
                        AuditAction.AIProviderFailoverTriggered,
                        "AIModel",
                        provider.Id,
                        $"Model '{modelToUse}' on provider '{provider.Name}' failed: {ex.Message}. Falling back to next available model/provider.",
                        token);
                }
            }
        }

        throw new InvalidOperationException("All AI providers and models failed.", lastException);
    }

    public async Task<string?> RewriteQueryWithFailoverAsync(string questionText, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(questionText)) return null;

        var config = await _configurationRepository.GetAsync(token);
        if (config == null) return null;

        var providers = (await _providerRepository.GetActiveAsync(token)).ToList();
        if (!providers.Any()) return null;

        var orderedProviders = providers
            .OrderBy(p => p.Id == config.ActiveProviderId ? 0 : 1)
            .ThenBy(p => p.FallbackPriority)
            .ToList();

        var triedProviders = new HashSet<Guid>();

        // Bound total rewrite time to 3.5 seconds
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(3.5));
        var linkedToken = timeoutCts.Token;

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

            var candidateModels = new List<string>();
            if (provider.Id == config.ActiveProviderId && config.ActiveModel != null)
            {
                candidateModels.Add(config.ActiveModel.ModelName);
                if (provider.Models != null)
                {
                    var fallbackModels = provider.Models
                        .Where(m => m.IsActive && m.Id != config.ActiveModelId)
                        .Select(m => m.ModelName)
                        .Distinct();
                    candidateModels.AddRange(fallbackModels);
                }
            }
            else if (provider.Models != null && provider.Models.Any(m => m.IsActive))
            {
                candidateModels.AddRange(provider.Models.Where(m => m.IsActive).Select(m => m.ModelName).Distinct());
            }

            if (!candidateModels.Any())
            {
                candidateModels.Add(config.ActiveModel?.ModelName ?? "default");
            }

            foreach (var modelToUse in candidateModels)
            {
                try
                {
                    var rewritten = await client.RewriteQueryAsync(questionText, modelToUse, linkedToken);
                    if (!string.IsNullOrWhiteSpace(rewritten))
                    {
                        return rewritten.Trim();
                    }
                }
                catch (OperationCanceledException) when (!token.IsCancellationRequested)
                {
                    // 3.5s bounded timeout reached for rewrite
                    if (!config.EnableAutoFailover)
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    if (!config.EnableAutoFailover)
                    {
                        return null;
                    }

                    await _auditService.LogAsync(
                        Guid.Empty,
                        AuditAction.AIProviderFailoverTriggered,
                        "AIModel",
                        provider.Id,
                        $"Query rewrite on model '{modelToUse}' (provider '{provider.Name}') failed: {ex.Message}. Falling back to next available model/provider.",
                        token);
                }
            }
        }

        return null;
    }
}
