using FluentValidation;

namespace Intervue.Application.Features.Interview.GenerateFeedback;

public class GenerateFeedbackValidator : AbstractValidator<GenerateFeedbackCommand>
{
    public GenerateFeedbackValidator()
    {
        RuleFor(x => x.InterviewId)
            .NotEmpty().WithMessage("InterviewId is required.");
    }
}
