using AIEmployeeSupport.Application.DTOs.AI;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class UpdateAIProviderRequestValidator : AbstractValidator<UpdateAIProviderRequest>
{
    private static readonly string[] ValidProviderTypes =
        { "OpenAI", "Gemini", "Anthropic", "AzureOpenAI", "Custom" };

    public UpdateAIProviderRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ø§Ø³Ù… Ø§Ù„Ù…Ø²ÙˆØ¯ Ù…Ø·Ù„ÙˆØ¨")
            .MaximumLength(200).WithMessage("Ø§Ø³Ù… Ø§Ù„Ù…Ø²ÙˆØ¯ ÙŠØ¬Ø¨ Ø£Ù„Ø§ ÙŠØªØ¬Ø§ÙˆØ² 200 Ø­Ø±Ù ");

        RuleFor(x => x.BaseUrl)
            .Must(UrlSecurityValidator.IsSafeUrl)
            .WithMessage("Base URL must be a valid HTTPS URL and cannot point to internal/private networks (SSRF protection).");
    }
}
