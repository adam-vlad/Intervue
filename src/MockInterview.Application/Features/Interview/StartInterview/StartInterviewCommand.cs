using MediatR;
using MockInterview.Application.Common;
using MockInterview.Application.Features.DTOs;

namespace MockInterview.Application.Features.Interview.StartInterview;

/// <summary>
/// Command to start a mock interview for a parsed CV profile.
/// The LLM generates the first interview question based on the CV.
/// </summary>
public record StartInterviewCommand(Guid CvProfileId) : IRequest<Result<InterviewDto>>;
