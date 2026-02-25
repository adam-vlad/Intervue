using MediatR;
using MockInterview.Application.Common;
using MockInterview.Application.Features.DTOs;

namespace MockInterview.Application.Features.Interview.GetInterview;

/// <summary>
/// Query to get an interview by its Id, including all messages and feedback.
/// </summary>
public record GetInterviewQuery(Guid InterviewId) : IRequest<Result<InterviewDto>>;
