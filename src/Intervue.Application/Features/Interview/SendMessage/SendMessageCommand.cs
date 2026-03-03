using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;

namespace Intervue.Application.Features.Interview.SendMessage;

/// <summary>
/// Command to send a candidate's answer and get a follow-up question from the AI.
/// </summary>
public record SendMessageCommand(Guid InterviewId, string Content) : IRequest<Result<InterviewMessageDto>>;
