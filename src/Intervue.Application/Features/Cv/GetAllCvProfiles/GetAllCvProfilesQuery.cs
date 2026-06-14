using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;

namespace Intervue.Application.Features.Cv.GetAllCvProfiles;

/// <summary>
/// Query to retrieve all CV profiles as lightweight summaries for the dashboard.
/// </summary>
public record GetAllCvProfilesQuery : IRequest<Result<IReadOnlyList<CvProfileSummaryDto>>>;
