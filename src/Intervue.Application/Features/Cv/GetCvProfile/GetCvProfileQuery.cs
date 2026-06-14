using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;

namespace Intervue.Application.Features.Cv.GetCvProfile;

/// <summary>
/// Query to get a CV profile by its Id, including all technologies, experiences, and projects.
/// </summary>
public record GetCvProfileQuery(Guid CvProfileId) : IRequest<Result<CvProfileDto>>;
