using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;

namespace Intervue.Application.Features.Interview.GenerateFeedback;

/// <summary>
/// Command to generate a feedback report for a completed interview.
/// The LLM analyzes the conversation and produces scores, strengths, weaknesses, and suggestions.
/// </summary>
public record GenerateFeedbackCommand(Guid InterviewId) : IRequest<Result<FeedbackReportDto>>;
