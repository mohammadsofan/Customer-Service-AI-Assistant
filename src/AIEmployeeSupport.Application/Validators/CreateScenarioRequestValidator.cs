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
                if (request.Status == "Active")
                    return steps != null && steps.Count > 0;
                return true;
            })
            .WithMessage("يجب إضافة خطوة حل واحدة على الأقل عند تفعيل السيناريو");

        RuleFor(x => x.Status)
            .Must(s => s is "Draft" or "Active" or "Inactive" or "Archived")
            .WithMessage("حالة السيناريو غير صالحة");
    }
}
