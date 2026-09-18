using AIEmployeeSupport.Application.DTOs.Common;
using FluentValidation;

namespace AIEmployeeSupport.Application.Validators;

public class PaginatedRequestValidator : AbstractValidator<PaginatedRequest>
{
    public PaginatedRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Ø§Ù„ØµÙ ØØ© ÙŠØ¬Ø¨ Ø£Ù† ØªÙƒÙˆÙ† Ø£ÙƒØ¨Ø± Ù…Ù† ØµÙ Ø±");
            
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Ø­Ø¬Ù… Ø§Ù„ØµÙ ØØ© ÙŠØ¬Ø¨ Ø£Ù† ÙŠÙƒÙˆÙ† Ø£ÙƒØ¨Ø± Ù…Ù† ØµÙ Ø±")
            .LessThanOrEqualTo(100).WithMessage("Ø­Ø¬Ù… Ø§Ù„ØµÙ ØØ© Ù„Ø§ ÙŠÙ…ÙƒÙ† Ø£Ù† ÙŠØªØ¬Ø§ÙˆØ² 100");
    }
}
