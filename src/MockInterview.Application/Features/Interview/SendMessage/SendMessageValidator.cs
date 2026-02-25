using FluentValidation;

namespace MockInterview.Application.Features.Interview.SendMessage;

public class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.InterviewId)
            .NotEmpty().WithMessage("InterviewId is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required.")
            .MaximumLength(5000).WithMessage("Message content must not exceed 5000 characters.");
    }
}
