using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;

namespace Intervue.Application.Features.Interview.GetInterviewsByCv;

/// <summary>
/// Query to retrieve all interviews for a specific CV profile.
/// </summary>
public record GetInterviewsByCvQuery(Guid CvProfileId) : IRequest<Result<IReadOnlyList<InterviewSummaryDto>>>;
