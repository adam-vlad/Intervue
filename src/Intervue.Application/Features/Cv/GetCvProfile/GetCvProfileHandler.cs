using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common;
using Intervue.Application.Common.Constants;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Repositories;

namespace Intervue.Application.Features.Cv.GetCvProfile;

/// <summary>
/// Handles GetCvProfileQuery — retrieves the CV profile from repository and maps to DTO.
/// </summary>
public class GetCvProfileHandler : IRequestHandler<GetCvProfileQuery, Result<CvProfileDto>>
{
    private readonly ICvProfileRepository _cvProfileRepository;
    private readonly ILogger<GetCvProfileHandler> _logger;

    public GetCvProfileHandler(ICvProfileRepository cvProfileRepository, ILogger<GetCvProfileHandler> logger)
    {
        _cvProfileRepository = cvProfileRepository;
        _logger = logger;
    }

    public async Task<Result<CvProfileDto>> Handle(GetCvProfileQuery request, CancellationToken cancellationToken)
    {
        var cvProfile = await _cvProfileRepository.GetByIdAsync(request.CvProfileId, cancellationToken);

        if (cvProfile is null)
        {
            _logger.LogWarning("CV profile with id {CvProfileId} was not found.", request.CvProfileId);
            return Result<CvProfileDto>.Fail(
                Error.NotFound(ErrorCodes.CvNotFound, $"CV profile with id '{request.CvProfileId}' was not found."));
        }

        return Result<CvProfileDto>.Ok(cvProfile.ToDto());
    }
}
