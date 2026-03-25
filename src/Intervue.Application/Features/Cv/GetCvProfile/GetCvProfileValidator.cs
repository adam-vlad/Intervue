using FluentValidation;

namespace Intervue.Application.Features.Cv.GetCvProfile;

/// <summary>
/// Validates GetCvProfileQuery — CvProfileId must not be empty.
/// </summary>
public class GetCvProfileValidator : AbstractValidator<GetCvProfileQuery>
{
    public GetCvProfileValidator()
    {
        RuleFor(x => x.CvProfileId)
            .NotEmpty().WithMessage("CvProfileId is required.");
    }
}
