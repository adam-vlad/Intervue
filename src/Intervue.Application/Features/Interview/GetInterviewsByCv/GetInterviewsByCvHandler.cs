using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common;
using Intervue.Application.Common.Constants;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Repositories;

namespace Intervue.Application.Features.Interview.GetInterviewsByCv;

/// <summary>
/// Handles GetInterviewsByCvQuery — retrieves interviews for a CV profile and maps to summary DTOs.
/// </summary>
public class GetInterviewsByCvHandler : IRequestHandler<GetInterviewsByCvQuery, Result<IReadOnlyList<InterviewSummaryDto>>>
{
    private readonly ICvProfileRepository _cvProfileRepository;
    private readonly IInterviewRepository _interviewRepository;
    private readonly ILogger<GetInterviewsByCvHandler> _logger;

    public GetInterviewsByCvHandler(
        ICvProfileRepository cvProfileRepository,
        IInterviewRepository interviewRepository,
        ILogger<GetInterviewsByCvHandler> logger)
    {
        _cvProfileRepository = cvProfileRepository;
        _interviewRepository = interviewRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<InterviewSummaryDto>>> Handle(GetInterviewsByCvQuery request, CancellationToken cancellationToken)
    {
        var cvProfile = await _cvProfileRepository.GetByIdAsync(request.CvProfileId, cancellationToken);

        if (cvProfile is null)
        {
            _logger.LogWarning("CV profile with id {CvProfileId} was not found.", request.CvProfileId);
            return Result<IReadOnlyList<InterviewSummaryDto>>.Fail(
                Error.NotFound(ErrorCodes.CvNotFound, $"CV profile with id '{request.CvProfileId}' was not found."));
        }

        var interviews = await _interviewRepository.GetByCvProfileIdAsync(request.CvProfileId, cancellationToken);

        _logger.LogInformation("Retrieved {Count} interviews for CV profile {CvProfileId}.", interviews.Count, request.CvProfileId);

        IReadOnlyList<InterviewSummaryDto> dtos = interviews
            .Select(i => i.ToSummaryDto())
            .ToList();

        return Result<IReadOnlyList<InterviewSummaryDto>>.Ok(dtos);
    }
}
