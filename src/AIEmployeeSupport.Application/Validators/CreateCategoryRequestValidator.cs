using AIEmployeeSupport.Application.DTOs.Knowledge;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم التصنيف مطلوب")
            .MaximumLength(200).WithMessage("اسم التصنيف يجب ألا يتجاوز 200 حرف");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("وصف التصنيف يجب ألا يتجاوز 1000 حرف");
    }
}
