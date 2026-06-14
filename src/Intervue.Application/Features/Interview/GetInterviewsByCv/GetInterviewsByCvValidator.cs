using FluentValidation;

namespace Intervue.Application.Features.Interview.GetInterviewsByCv;

/// <summary>
/// Validates GetInterviewsByCvQuery — CvProfileId must not be empty.
/// </summary>
public class GetInterviewsByCvValidator : AbstractValidator<GetInterviewsByCvQuery>
{
    public GetInterviewsByCvValidator()
    {
        RuleFor(x => x.CvProfileId)
            .NotEmpty().WithMessage("CvProfileId is required.");
    }
}
