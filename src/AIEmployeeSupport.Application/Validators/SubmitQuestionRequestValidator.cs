using AIEmployeeSupport.Application.DTOs.Support;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class SubmitQuestionRequestValidator : AbstractValidator<SubmitQuestionRequest>
{
    public SubmitQuestionRequestValidator()
    {
        RuleFor(x => x.Problem)
            .NotEmpty().WithMessage("نص المشكلة مطلوب")
            .MinimumLength(5).WithMessage("نص المشكلة يجب أن يكون 5 أحرف على الأقل")
            .MaximumLength(2000).WithMessage("نص المشكلة يجب ألا يتجاوز 2000 حرف");
    }
}
