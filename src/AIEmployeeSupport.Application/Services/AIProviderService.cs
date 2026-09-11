using AIEmployeeSupport.Application.DTOs.AI;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;

namespace AIEmployeeSupport.Application.Services;

public class AIProviderService : IAIProviderService
{
    private readonly IAIProviderRepository _providerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly IAIProviderFactory _providerFactory;

    public AIProviderService(
        IAIProviderRepository providerRepository,
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        IAIProviderFactory providerFactory)
    {
        _providerRepository = providerRepository;
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
            IsActive = true,
            FallbackPriority = 10,
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
        provider.FallbackPriority = request.FallbackPriority;
        provider.UpdatedAt = DateTime.UtcNow;

        await _providerRepository.UpdateAsync(provider, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(provider);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
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

        var apiKey = _encryptionService.Decrypt(provider.EncryptedApiKey);
        var client = _providerFactory.CreateClient(provider.ProviderType, apiKey);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // Simply attempt a basic generate to test connection
            var response = await client.GenerateAnswerAsync(new AIRequest 
            { 
                QuestionText = "Test connection",
                ModelName = "default"
            }, cancellationToken);
            
            return new ProviderTestResult 
            { 
                Success = true, 
                Message = "Connection successful", 
                LatencyMs = stopwatch.ElapsedMilliseconds 
            };
        }
        catch (Exception ex)
        {
            return new ProviderTestResult 
            { 
                Success = false, 
                Message = $"Connection failed: {ex.Message}", 
                LatencyMs = stopwatch.ElapsedMilliseconds 
            };
        }
    }

    private AIProviderDto MapToDto(AIProvider provider)
    {
        return new AIProviderDto
        {
            Id = provider.Id,
            Name = provider.Name,
            ProviderType = provider.ProviderType.ToString(),
            IsActive = provider.IsActive,
            FallbackPriority = provider.FallbackPriority,
            MaskedApiKey = MaskApiKey(_encryptionService.Decrypt(provider.EncryptedApiKey)),
            ModelCount = provider.Models?.Count ?? 0,
            CreatedAt = provider.CreatedAt
        };
    }

    private string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey)) return string.Empty;
        if (apiKey.Length <= 6) return new string('*', apiKey.Length);
        return apiKey.Substring(0, 3) + new string('*', apiKey.Length - 6) + apiKey.Substring(apiKey.Length - 3);
    }
}
