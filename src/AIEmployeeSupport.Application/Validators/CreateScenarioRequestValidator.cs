using AIEmployeeSupport.Application.DTOs.Knowledge;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class CreateScenarioRequestValidator : AbstractValidator<CreateScenarioRequest>
{
    public CreateScenarioRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم السيناريو مطلوب")
            .MaximumLength(300).WithMessage("اسم السيناريو يجب ألا يتجاوز 300 حرف");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("وصف السيناريو مطلوب")
            .MaximumLength(4000).WithMessage("وصف السيناريو يجب ألا يتجاوز 4000 حرف");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("التصنيف مطلوب");

        RuleFor(x => x.ResolutionSteps)
            .Must((request, steps) =>
            {
                var isActive = string.IsNullOrEmpty(request.Status) ||
                               string.Equals(request.Status, "Active", StringComparison.OrdinalIgnoreCase) ||
                               request.Status == "1" ||
                               request.Status == "نشط";
                if (isActive)
                {
                    var hasLegacySteps = steps != null && steps.Any(s => !string.IsNullOrWhiteSpace(s));
                    var hasNewSteps = request.Steps != null && request.Steps.Any(s => !string.IsNullOrWhiteSpace(s.StepText));
                    return hasLegacySteps || hasNewSteps;
                }
                return true;
            })
            .WithMessage("يجب إضافة خطوة حل واحدة على الأقل عند تفعيل السيناريو");

        RuleFor(x => x.Status)
            .Must(s => string.IsNullOrEmpty(s) ||
                       s.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
                       s.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                       s.Equals("Inactive", StringComparison.OrdinalIgnoreCase) ||
                       s.Equals("Archived", StringComparison.OrdinalIgnoreCase) ||
                       s == "نشط" || s == "مسودة" || s == "غير نشط")
            .WithMessage("حالة السيناريو غير صالحة");
    }
}
