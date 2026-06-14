using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;

namespace Intervue.Application.Features.Interview.GetInterview;

/// <summary>
/// Query to get an interview by its Id, including all messages and feedback.
/// </summary>
public record GetInterviewQuery(Guid InterviewId) : IRequest<Result<InterviewDto>>;
