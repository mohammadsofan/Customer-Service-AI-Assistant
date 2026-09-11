using AIEmployeeSupport.Application.DTOs.Knowledge;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class UpdateScenarioRequestValidator : AbstractValidator<UpdateScenarioRequest>
{
    public UpdateScenarioRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم السيناريو مطلوب")
            .MaximumLength(300).WithMessage("اسم السيناريو يجب ألا يتجاوز 300 حرف");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("وصف السيناريو مطلوب")
            .MaximumLength(4000).WithMessage("وصف السيناريو يجب ألا يتجاوز 4000 حرف");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("التصنيف مطلوب");
    }
}
