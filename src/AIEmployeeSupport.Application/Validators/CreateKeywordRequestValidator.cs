using AIEmployeeSupport.Application.DTOs.Knowledge;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class CreateKeywordRequestValidator : AbstractValidator<CreateKeywordRequest>
{
    public CreateKeywordRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم الكلمة المفتاحية مطلوب")
            .MaximumLength(100).WithMessage("اسم الكلمة المفتاحية يجب ألا يتجاوز 100 حرف");
    }
}
