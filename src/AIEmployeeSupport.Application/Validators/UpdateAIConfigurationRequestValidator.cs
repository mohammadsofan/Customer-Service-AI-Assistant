using AIEmployeeSupport.Application.DTOs.AI;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class UpdateAIConfigurationRequestValidator : AbstractValidator<UpdateAIConfigurationRequest>
{
    public UpdateAIConfigurationRequestValidator()
    {
        RuleFor(x => x.ActiveProviderId)
            .NotEmpty().WithMessage("معرّف المزود النشط مطلوب");

        RuleFor(x => x.ActiveModelId)
            .NotEmpty().WithMessage("معرّف النموذج النشط مطلوب");

        RuleFor(x => x.Temperature)
            .InclusiveBetween(0, 2).WithMessage("درجة الحرارة يجب أن تكون بين 0 و 2");

        RuleFor(x => x.MaxTokens)
            .GreaterThan(0).WithMessage("الحد الأقصى للرموز يجب أن يكون أكبر من 0");

        RuleFor(x => x.SimilarityThreshold)
            .InclusiveBetween(0, 1).WithMessage("عتبة التشابه يجب أن تكون بين 0 و 1");

        RuleFor(x => x.TopK)
            .GreaterThan(0).WithMessage("قيمة TopK يجب أن تكون أكبر من 0");

        RuleFor(x => x.SystemPrompt)
            .NotEmpty().WithMessage("موجه النظام مطلوب");
    }
}
