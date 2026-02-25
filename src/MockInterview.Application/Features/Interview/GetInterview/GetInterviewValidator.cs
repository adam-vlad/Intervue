using FluentValidation;

namespace MockInterview.Application.Features.Interview.GetInterview;

public class GetInterviewValidator : AbstractValidator<GetInterviewQuery>
{
    public GetInterviewValidator()
    {
        RuleFor(x => x.InterviewId)
            .NotEmpty().WithMessage("InterviewId is required.");
    }
}
