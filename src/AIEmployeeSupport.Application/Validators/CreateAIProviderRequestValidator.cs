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
            .Must(t => ValidProviderTypes.Any(v => string.Equals(v, t, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("نوع المزود غير صالح. القيم المسموحة: OpenAI, Gemini, Anthropic, AzureOpenAI, Custom");

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("مفتاح API مطلوب");

        RuleFor(x => x.BaseUrl)
            .Must(UrlSecurityValidator.IsSafeUrl)
            .WithMessage("Base URL must be a valid HTTPS URL and cannot point to internal/private networks (SSRF protection).");
    }
}
