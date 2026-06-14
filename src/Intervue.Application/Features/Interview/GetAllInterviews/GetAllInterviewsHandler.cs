using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Repositories;

namespace Intervue.Application.Features.Interview.GetAllInterviews;

/// <summary>
/// Handles GetAllInterviewsQuery — retrieves all interviews and maps to summary DTOs.
/// </summary>
public class GetAllInterviewsHandler : IRequestHandler<GetAllInterviewsQuery, Result<IReadOnlyList<InterviewSummaryDto>>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly ILogger<GetAllInterviewsHandler> _logger;

    public GetAllInterviewsHandler(IInterviewRepository interviewRepository, ILogger<GetAllInterviewsHandler> logger)
    {
        _interviewRepository = interviewRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<InterviewSummaryDto>>> Handle(GetAllInterviewsQuery request, CancellationToken cancellationToken)
    {
        var interviews = await _interviewRepository.GetAllAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} interviews.", interviews.Count);

        IReadOnlyList<InterviewSummaryDto> dtos = interviews
            .Select(i => i.ToSummaryDto())
            .OrderByDescending(i => i.StartedAt)
            .ToList();

        return Result<IReadOnlyList<InterviewSummaryDto>>.Ok(dtos);
    }
}
