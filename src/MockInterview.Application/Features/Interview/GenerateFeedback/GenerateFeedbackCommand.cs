using MediatR;
using MockInterview.Application.Common;
using MockInterview.Application.Features.DTOs;

namespace MockInterview.Application.Features.Interview.GenerateFeedback;

/// <summary>
/// Command to generate a feedback report for a completed interview.
/// The LLM analyzes the conversation and produces scores, strengths, weaknesses, and suggestions.
/// </summary>
public record GenerateFeedbackCommand(Guid InterviewId) : IRequest<Result<FeedbackReportDto>>;
