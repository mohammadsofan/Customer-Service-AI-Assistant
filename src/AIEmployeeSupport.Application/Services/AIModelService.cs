using AIEmployeeSupport.Application.DTOs.AI;
using AIEmployeeSupport.Application.Exceptions;
using AIEmployeeSupport.Application.Interfaces;
using AIEmployeeSupport.Application.Interfaces.Services;
using AIEmployeeSupport.Domain.Entities;

namespace AIEmployeeSupport.Application.Services;

public class AIModelService : IAIModelService
{
    private readonly IAIModelRepository _modelRepository;
    private readonly IAIConfigurationRepository _configurationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public AIModelService(
        IAIModelRepository modelRepository,
        IAIConfigurationRepository configurationRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _modelRepository = modelRepository;
        _configurationRepository = configurationRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<IEnumerable<AIModelDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var models = await _modelRepository.GetAllAsync(cancellationToken);
        return models.Where(m => m.IsActive).Select(m => new AIModelDto
        {
            Id = m.Id,
            ProviderId = m.ProviderId,
            ModelName = m.ModelName,
            IsActive = m.IsActive
        });
    }

    public async Task<IEnumerable<AIModelDto>> GetByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var models = await _modelRepository.GetAllAsync(cancellationToken);
        return models.Where(m => m.ProviderId == providerId && m.IsActive).Select(m => new AIModelDto
        {
            Id = m.Id,
            ProviderId = m.ProviderId,
            ModelName = m.ModelName,
            IsActive = m.IsActive
        });
    }

    public async Task<AIModelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _modelRepository.GetByIdAsync(id, cancellationToken);
        if (model == null) return null;

        return new AIModelDto
        {
            Id = model.Id,
            ProviderId = model.ProviderId,
            ModelName = model.ModelName,
            IsActive = model.IsActive
        };
    }

    public async Task<AIModelDto> CreateAsync(CreateAIModelRequest request, CancellationToken cancellationToken = default)
    {
        var model = new AIModel
        {
            Id = Guid.NewGuid(),
            ProviderId = request.ProviderId,
            ModelName = request.ModelName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _modelRepository.CreateAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(model.Id, cancellationToken))!;
    }

    public async Task<AIModelDto> UpdateAsync(Guid id, string modelName, CancellationToken cancellationToken = default)
    {
        var model = await _modelRepository.GetByIdAsync(id, cancellationToken);
        if (model == null) throw new NotFoundException(nameof(AIModel), id);

        model.ModelName = modelName;
        model.UpdatedAt = DateTime.UtcNow;

        await _modelRepository.UpdateAsync(model, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(model.Id, cancellationToken))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _modelRepository.GetByIdAsync(id, cancellationToken);
        if (model == null) throw new NotFoundException(nameof(AIModel), id);

        var config = await _configurationRepository.GetAsync(cancellationToken);
        if (config != null && config.ActiveModelId == id)
        {
            throw new InvalidOperationException("لا يمكن حذف هذا النموذج لأنه محدد كالنموذج النشط حالياً في إعدادات النظام. يرجى اختيار نموذج نشط آخر أولاً.");
        }

        await _modelRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
