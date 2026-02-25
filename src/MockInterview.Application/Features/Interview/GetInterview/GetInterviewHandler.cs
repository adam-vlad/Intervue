using MediatR;
using MockInterview.Application.Common;
using MockInterview.Application.Features.DTOs;
using MockInterview.Domain.Repositories;

namespace MockInterview.Application.Features.Interview.GetInterview;

/// <summary>
/// Handles GetInterviewQuery — simply retrieves the interview from repository and maps to DTO.
/// </summary>
public class GetInterviewHandler : IRequestHandler<GetInterviewQuery, Result<InterviewDto>>
{
    private readonly IInterviewRepository _interviewRepository;

    public GetInterviewHandler(IInterviewRepository interviewRepository)
    {
        _interviewRepository = interviewRepository;
    }

    public async Task<Result<InterviewDto>> Handle(GetInterviewQuery request, CancellationToken cancellationToken)
    {
        var interview = await _interviewRepository.GetByIdAsync(request.InterviewId, cancellationToken);

        if (interview is null)
        {
            return Result<InterviewDto>.Fail(
                Error.NotFound("Interview.NotFound", $"Interview with id '{request.InterviewId}' was not found."));
        }

        return Result<InterviewDto>.Ok(interview.ToDto());
    }
}
