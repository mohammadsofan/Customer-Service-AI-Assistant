using AIEmployeeSupport.Application.DTOs.AI;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Services;

public class AIProviderService : IAIProviderService
{
    private readonly IAIProviderRepository _providerRepository;
    private readonly IAIConfigurationRepository _configurationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly IAIProviderFactory _providerFactory;

    public AIProviderService(
        IAIProviderRepository providerRepository,
        IAIConfigurationRepository configurationRepository,
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        IAIProviderFactory providerFactory)
    {
        _providerRepository = providerRepository;
        _configurationRepository = configurationRepository;
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
        _providerFactory = providerFactory;
    }

    public async Task<IEnumerable<AIProviderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var providers = await _providerRepository.GetAllAsync(cancellationToken);
        return providers.Select(MapToDto);
    }

    public async Task<AIProviderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _providerRepository.GetByIdAsync(id, cancellationToken);
        return provider == null ? null : MapToDto(provider);
    }

    public async Task<AIProviderDto> CreateAsync(CreateAIProviderRequest request, CancellationToken cancellationToken = default)
    {
        var provider = new AIProvider
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ProviderType = Enum.Parse<ProviderType>(request.ProviderType, true),
            EncryptedApiKey = _encryptionService.Encrypt(request.ApiKey),
            BaseUrl = request.BaseUrl,
            IsActive = true,
            FallbackPriority = request.FallbackPriority ?? 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _providerRepository.CreateAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(created);
    }

    public async Task<AIProviderDto> UpdateAsync(Guid id, UpdateAIProviderRequest request, CancellationToken cancellationToken = default)
    {
        var provider = await _providerRepository.GetByIdAsync(id, cancellationToken);
        if (provider == null) throw new InvalidOperationException("Provider not found");

        provider.Name = request.Name;
        if (!string.IsNullOrEmpty(request.ApiKey))
        {
            provider.EncryptedApiKey = _encryptionService.Encrypt(request.ApiKey);
        }
        if (request.BaseUrl != null)
        {
            provider.BaseUrl = request.BaseUrl;
        }
        provider.FallbackPriority = request.FallbackPriority;
        provider.UpdatedAt = DateTime.UtcNow;

        await _providerRepository.UpdateAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(provider);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var config = await _configurationRepository.GetAsync(cancellationToken);
        if (config != null && config.ActiveProviderId == id)
        {
            throw new InvalidOperationException("لا يمكن حذف هذا المزود لأنه محدد كالمزود النشط حالياً في إعدادات النظام. يرجى اختيار مزود نشط آخر أولاً.");
        }

        await _providerRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _providerRepository.GetByIdAsync(id, cancellationToken);
        if (provider == null) throw new InvalidOperationException("Provider not found");

        provider.IsActive = true;
        provider.UpdatedAt = DateTime.UtcNow;

        await _providerRepository.UpdateAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _providerRepository.GetByIdAsync(id, cancellationToken);
        if (provider == null) throw new InvalidOperationException("Provider not found");

        provider.IsActive = false;
        provider.UpdatedAt = DateTime.UtcNow;

        await _providerRepository.UpdateAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProviderTestResult> TestConnectionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var provider = await _providerRepository.GetByIdAsync(id, cancellationToken);
        if (provider == null) throw new InvalidOperationException("Provider not found");

        string apiKey = string.Empty;
        if (!string.IsNullOrWhiteSpace(provider.EncryptedApiKey))
        {
            try
            {
                apiKey = _encryptionService.Decrypt(provider.EncryptedApiKey);
            }
            catch
            {
                apiKey = provider.EncryptedApiKey;
            }
        }

        var client = _providerFactory.CreateClient(provider.ProviderType, apiKey, provider.BaseUrl);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var activeModel = provider.Models?.FirstOrDefault(m => m.IsActive)?.ModelName;
            var fallbackModel = provider.Models?.FirstOrDefault()?.ModelName;
            var modelToUse = !string.IsNullOrWhiteSpace(activeModel)
                ? activeModel
                : (!string.IsNullOrWhiteSpace(fallbackModel) ? fallbackModel : "gpt-4o-mini");

            // Attempt a basic request to test provider connection
            var response = await client.GenerateAnswerAsync(new AIRequest 
            { 
                QuestionText = "Test connection",
                ModelName = modelToUse,
                MaxTokens = 256,
                Temperature = 0.5
            }, cancellationToken);
            
            return new ProviderTestResult 
            { 
                Success = true, 
                Message = $"Connection successful ({stopwatch.ElapsedMilliseconds}ms)", 
                LatencyMs = stopwatch.ElapsedMilliseconds 
            };
        }
        catch (Exception ex)
        {
            return new ProviderTestResult 
            { 
                Success = false, 
                Message = ex.Message, 
                LatencyMs = stopwatch.ElapsedMilliseconds 
            };
        }
    }

    private static AIProviderDto MapToDto(AIProvider provider)
    {
        var hasKey = !string.IsNullOrWhiteSpace(provider.EncryptedApiKey);

        return new AIProviderDto
        {
            Id = provider.Id,
            Name = provider.Name,
            ProviderType = provider.ProviderType.ToString(),
            BaseUrl = provider.BaseUrl,
            IsActive = provider.IsActive,
            FallbackPriority = provider.FallbackPriority,
            HasApiKey = hasKey,
            MaskedApiKey = hasKey ? "••••••••" : string.Empty,
            ModelCount = provider.Models?.Count ?? 0,
            CreatedAt = provider.CreatedAt
        };
    }
}
