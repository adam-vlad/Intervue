using MediatR;
using MockInterview.Application.Common;
using MockInterview.Application.Common.Interfaces;
using MockInterview.Application.Features.DTOs;
using MockInterview.Domain.Enums;
using MockInterview.Domain.Repositories;

namespace MockInterview.Application.Features.Interview.SendMessage;

/// <summary>
/// Handles SendMessageCommand:
/// 1. Gets the Interview from repository
/// 2. Adds the candidate's message
/// 3. Sends the full conversation history to the LLM for a follow-up question
/// 4. Adds the follow-up question as an interviewer message
/// 5. Returns the new follow-up question as InterviewMessageDto
/// </summary>
public class SendMessageHandler : IRequestHandler<SendMessageCommand, Result<InterviewMessageDto>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly ICvProfileRepository _cvProfileRepository;
    private readonly ILlmClient _llmClient;

    public SendMessageHandler(
        IInterviewRepository interviewRepository,
        ICvProfileRepository cvProfileRepository,
        ILlmClient llmClient)
    {
        _interviewRepository = interviewRepository;
        _cvProfileRepository = cvProfileRepository;
        _llmClient = llmClient;
    }

    public async Task<Result<InterviewMessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Get the interview
        var interview = await _interviewRepository.GetByIdAsync(request.InterviewId, cancellationToken);

        if (interview is null)
        {
            return Result<InterviewMessageDto>.Fail(
                Error.NotFound("Interview.NotFound", $"Interview with id '{request.InterviewId}' was not found."));
        }

        // Step 2: Add the candidate's message
        interview.AddCandidateMessage(request.Content);

        // Step 3: Build conversation history for the LLM
        var cvProfile = await _cvProfileRepository.GetByIdAsync(interview.CvProfileId, cancellationToken);

        var llmMessages = new List<LlmMessage>
        {
            new("system", $"""
                You are a professional technical interviewer conducting a mock interview.
                The candidate's level is {cvProfile?.DifficultyLevel ?? DifficultyLevel.Junior}.
                
                Rules:
                - Ask focused, follow-up questions based on the candidate's answers
                - If the answer is vague, ask for specific examples or deeper explanation
                - If the answer is good, move to a related but different topic
                - Keep questions concise and clear
                - Be professional and encouraging
                - Respond with ONLY your next question, nothing else
                """)
        };

        // Add the full conversation history so the LLM has context
        foreach (var msg in interview.Messages)
        {
            var role = msg.Role == MessageRole.Interviewer ? "assistant" : "user";
            llmMessages.Add(new LlmMessage(role, msg.Content));
        }

        // Step 4: Get follow-up question from LLM
        var followUp = await _llmClient.ChatAsync(llmMessages, cancellationToken);

        // Step 5: Add the follow-up question
        interview.AddInterviewerMessage(followUp);

        // Step 6: Save changes
        await _interviewRepository.UpdateAsync(interview, cancellationToken);

        // Step 7: Return the new follow-up message
        var lastMessage = interview.Messages[^1];
        return Result<InterviewMessageDto>.Ok(lastMessage.ToDto());
    }
}
