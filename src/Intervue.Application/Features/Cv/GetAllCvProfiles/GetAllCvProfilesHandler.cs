using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Repositories;

namespace Intervue.Application.Features.Cv.GetAllCvProfiles;

/// <summary>
/// Handles GetAllCvProfilesQuery — retrieves all CV profiles and maps to summary DTOs.
/// </summary>
public class GetAllCvProfilesHandler : IRequestHandler<GetAllCvProfilesQuery, Result<IReadOnlyList<CvProfileSummaryDto>>>
{
    private readonly ICvProfileRepository _cvProfileRepository;
    private readonly ILogger<GetAllCvProfilesHandler> _logger;

    public GetAllCvProfilesHandler(ICvProfileRepository cvProfileRepository, ILogger<GetAllCvProfilesHandler> logger)
    {
        _cvProfileRepository = cvProfileRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<CvProfileSummaryDto>>> Handle(GetAllCvProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _cvProfileRepository.GetAllAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} CV profiles.", profiles.Count);

        IReadOnlyList<CvProfileSummaryDto> dtos = profiles
            .Select(p => p.ToSummaryDto())
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return Result<IReadOnlyList<CvProfileSummaryDto>>.Ok(dtos);
    }
}
