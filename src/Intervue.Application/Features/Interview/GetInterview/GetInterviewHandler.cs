using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common;
using Intervue.Application.Common.Constants;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Repositories;

namespace Intervue.Application.Features.Interview.GetInterview;

/// <summary>
/// Handles GetInterviewQuery — simply retrieves the interview from repository and maps to DTO.
/// </summary>
public class GetInterviewHandler : IRequestHandler<GetInterviewQuery, Result<InterviewDto>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly ILogger<GetInterviewHandler> _logger;

    public GetInterviewHandler(IInterviewRepository interviewRepository, ILogger<GetInterviewHandler> logger)
    {
        _interviewRepository = interviewRepository;
        _logger = logger;
    }

    public async Task<Result<InterviewDto>> Handle(GetInterviewQuery request, CancellationToken cancellationToken)
    {
        var interview = await _interviewRepository.GetByIdAsync(request.InterviewId, cancellationToken);

        if (interview is null)
        {
            _logger.LogWarning("Interview with id {InterviewId} was not found.", request.InterviewId);
            return Result<InterviewDto>.Fail(
                Error.NotFound(ErrorCodes.InterviewNotFound, $"Interview with id '{request.InterviewId}' was not found."));
        }

        return Result<InterviewDto>.Ok(interview.ToDto());
    }
}
