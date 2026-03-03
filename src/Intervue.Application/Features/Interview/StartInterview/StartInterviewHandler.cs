using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Repositories;

namespace Intervue.Application.Features.Interview.StartInterview;

/// <summary>
/// Handles StartInterviewCommand:
/// 1. Gets the CvProfile from repository
/// 2. Creates an Interview entity
/// 3. Asks the LLM to generate the first question based on the CV
/// 4. Starts the interview with the first question
/// 5. Saves and returns the InterviewDto
/// </summary>
public class StartInterviewHandler : IRequestHandler<StartInterviewCommand, Result<InterviewDto>>
{
    private readonly ICvProfileRepository _cvProfileRepository;
    private readonly IInterviewRepository _interviewRepository;
    private readonly ILlmClient _llmClient;

    public StartInterviewHandler(
        ICvProfileRepository cvProfileRepository,
        IInterviewRepository interviewRepository,
        ILlmClient llmClient)
    {
        _cvProfileRepository = cvProfileRepository;
        _interviewRepository = interviewRepository;
        _llmClient = llmClient;
    }

    public async Task<Result<InterviewDto>> Handle(StartInterviewCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Get the CV profile
        var cvProfile = await _cvProfileRepository.GetByIdAsync(request.CvProfileId, cancellationToken);

        if (cvProfile is null)
        {
            return Result<InterviewDto>.Fail(
                Error.NotFound("Cv.NotFound", $"CV profile with id '{request.CvProfileId}' was not found."));
        }

        // Step 2: Create the Interview entity
        var interview = Domain.Entities.Interview.Create(request.CvProfileId);

        // Step 3: Build a prompt for the first question
        var techSummary = cvProfile.Technologies.Any()
            ? string.Join(", ", cvProfile.Technologies.Select(t => $"{t.Name} ({t.YearsOfExperience}y)"))
            : "not specified";

        var expSummary = cvProfile.Experiences.Any()
            ? string.Join("; ", cvProfile.Experiences.Select(e => $"{e.Role} at {e.Company}"))
            : "not specified";

        var prompt = $"""
            You are a technical interviewer conducting a mock interview.
            
            Candidate profile:
            - Difficulty level: {cvProfile.DifficultyLevel}
            - Technologies: {techSummary}
            - Experience: {expSummary}
            - Education: {cvProfile.Education ?? "not specified"}
            
            Generate your first interview question. The question should:
            - Be appropriate for the candidate's level ({cvProfile.DifficultyLevel})
            - Focus on one of their main technologies
            - Be open-ended to encourage discussion
            - Be professional and welcoming
            
            Start with a brief greeting, then ask your first question.
            Respond with ONLY the greeting and question, nothing else.
            """;

        var messages = new List<LlmMessage>
        {
            new("system", "You are a professional technical interviewer. Be friendly but thorough."),
            new("user", prompt)
        };

        var firstQuestion = await _llmClient.ChatAsync(messages, cancellationToken);

        // Step 4: Start the interview with the first question
        interview.Start(firstQuestion);

        // Step 5: Save to repository
        await _interviewRepository.AddAsync(interview, cancellationToken);

        // Step 6: Return the DTO
        return Result<InterviewDto>.Created(interview.ToDto());
    }
}
