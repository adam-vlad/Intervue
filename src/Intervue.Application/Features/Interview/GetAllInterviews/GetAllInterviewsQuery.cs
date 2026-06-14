using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;

namespace Intervue.Application.Features.Interview.GetAllInterviews;

/// <summary>
/// Query to retrieve all interviews as lightweight summaries for the dashboard.
/// </summary>
public record GetAllInterviewsQuery : IRequest<Result<IReadOnlyList<InterviewSummaryDto>>>;
