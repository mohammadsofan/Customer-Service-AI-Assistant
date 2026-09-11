using AIEmployeeSupport.Application.DTOs.AI;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class CreateAIProviderRequestValidator : AbstractValidator<CreateAIProviderRequest>
{
    private static readonly string[] ValidProviderTypes =
        { "OpenAI", "Gemini", "Anthropic", "AzureOpenAI", "Custom" };

    public CreateAIProviderRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المزود مطلوب")
            .MaximumLength(200).WithMessage("اسم المزود يجب ألا يتجاوز 200 حرف");

        RuleFor(x => x.ProviderType)
            .NotEmpty().WithMessage("نوع المزود مطلوب")
            .Must(t => ValidProviderTypes.Contains(t))
            .WithMessage("نوع المزود غير صالح. القيم المسموحة: OpenAI, Gemini, Anthropic, AzureOpenAI, Custom");

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("مفتاح API مطلوب");
    }
}
