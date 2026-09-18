using AIEmployeeSupport.Application.DTOs.AI;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;

namespace AIEmployeeSupport.Application.Services;

public class AIConfigurationService : IAIConfigurationService
{
    private readonly IAIConfigurationRepository _configurationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AIConfigurationService(IAIConfigurationRepository configurationRepository, IUnitOfWork unitOfWork)
    {
        _configurationRepository = configurationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AIConfigurationDto?> GetAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configurationRepository.GetAsync(cancellationToken);
        if (config == null) return null;

        return new AIConfigurationDto
        {
            ActiveProviderId = config.ActiveProviderId,
            ActiveProviderName = config.ActiveProvider?.Name ?? string.Empty,
            ActiveModelId = config.ActiveModelId,
            ActiveModelName = config.ActiveModel?.ModelName ?? string.Empty,
            ActiveEmbeddingProviderId = config.ActiveEmbeddingProviderId,
            ActiveEmbeddingProviderName = config.ActiveEmbeddingProvider?.Name,
            ActiveEmbeddingModelId = config.ActiveEmbeddingModelId,
            ActiveEmbeddingModelName = config.ActiveEmbeddingModel?.ModelName,
            Temperature = config.Temperature,
            MaxTokens = config.MaxTokens,
            SimilarityThreshold = config.SimilarityThreshold,
            TopK = config.TopK,
            SystemPrompt = config.SystemPrompt,
            EnableAutoFailover = config.EnableAutoFailover
        };
    }

    public async Task<AIConfigurationDto> UpdateAsync(Guid userId, UpdateAIConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var config = await _configurationRepository.GetAsync(cancellationToken);
        if (config == null) throw new InvalidOperationException("Configuration not found");

        config.ActiveProviderId = request.ActiveProviderId;
        config.ActiveModelId = request.ActiveModelId;
        config.ActiveEmbeddingProviderId = request.ActiveEmbeddingProviderId;
        config.ActiveEmbeddingModelId = request.ActiveEmbeddingModelId;
        config.Temperature = request.Temperature;
        config.MaxTokens = request.MaxTokens;
        config.SimilarityThreshold = request.SimilarityThreshold;
        config.TopK = request.TopK;
        config.SystemPrompt = request.SystemPrompt;
        config.EnableAutoFailover = request.EnableAutoFailover;
        config.UpdatedBy = userId;
        config.UpdatedAt = DateTime.UtcNow;

        await _configurationRepository.UpdateAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetAsync(cancellationToken) ?? throw new InvalidOperationException("Failed to retrieve updated configuration");
    }
}
