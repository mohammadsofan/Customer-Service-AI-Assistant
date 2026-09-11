using AIEmployeeSupport.Application.DTOs.AI;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class CreateAIModelRequestValidator : AbstractValidator<CreateAIModelRequest>
{
    public CreateAIModelRequestValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("معرّف المزود مطلوب");

        RuleFor(x => x.ModelName)
            .NotEmpty().WithMessage("اسم النموذج مطلوب")
            .MaximumLength(200).WithMessage("اسم النموذج يجب ألا يتجاوز 200 حرف");
    }
}
