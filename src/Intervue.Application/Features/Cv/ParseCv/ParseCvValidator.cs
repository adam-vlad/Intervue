using FluentValidation;

namespace Intervue.Application.Features.Cv.ParseCv;

/// <summary>
/// Validates ParseCvCommand — CvProfileId must not be empty.
/// </summary>
public class ParseCvValidator : AbstractValidator<ParseCvCommand>
{
    public ParseCvValidator()
    {
        RuleFor(x => x.CvProfileId)
            .NotEmpty().WithMessage("CvProfileId is required.");
    }
}
