using FluentValidation;

namespace Intervue.Application.Features.Interview.StartInterview;

public class StartInterviewValidator : AbstractValidator<StartInterviewCommand>
{
    public StartInterviewValidator()
    {
        RuleFor(x => x.CvProfileId)
            .NotEmpty().WithMessage("CvProfileId is required.");
    }
}
