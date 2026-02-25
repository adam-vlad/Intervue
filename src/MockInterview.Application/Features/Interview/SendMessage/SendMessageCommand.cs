using MediatR;
using MockInterview.Application.Common;
using MockInterview.Application.Features.DTOs;

namespace MockInterview.Application.Features.Interview.SendMessage;

/// <summary>
/// Command to send a candidate's answer and get a follow-up question from the AI.
/// </summary>
public record SendMessageCommand(Guid InterviewId, string Content) : IRequest<Result<InterviewMessageDto>>;
