using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;

namespace Intervue.Application.Features.Interview.StartInterview;

/// <summary>
/// Command to start a mock interview for a parsed CV profile.
/// The LLM generates the first interview question based on the CV.
/// </summary>
public record StartInterviewCommand(Guid CvProfileId) : IRequest<Result<InterviewDto>>;
