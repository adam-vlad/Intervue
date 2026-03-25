using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common;
using Intervue.Application.Common.Constants;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Common.Prompts;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;

namespace Intervue.Application.Features.Interview.SendMessage;

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
    private readonly ILogger<SendMessageHandler> _logger;

    public SendMessageHandler(
        IInterviewRepository interviewRepository,
        ICvProfileRepository cvProfileRepository,
        ILlmClient llmClient,
        ILogger<SendMessageHandler> logger)
    {
        _interviewRepository = interviewRepository;
        _cvProfileRepository = cvProfileRepository;
        _llmClient = llmClient;
        _logger = logger;
    }

    public async Task<Result<InterviewMessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Get the interview
        var interview = await _interviewRepository.GetByIdAsync(request.InterviewId, cancellationToken);

        if (interview is null)
        {
            return Result<InterviewMessageDto>.Fail(
                Error.NotFound(ErrorCodes.InterviewNotFound, $"Interview with id '{request.InterviewId}' was not found."));
        }

        // Step 2: Add the candidate's message
        interview.AddCandidateMessage(request.Content);

        // Step 3: Build conversation history for the LLM
        var cvProfile = await _cvProfileRepository.GetByIdAsync(interview.CvProfileId, cancellationToken);

        var difficultyLevel = cvProfile?.DifficultyLevel ?? DifficultyLevel.Junior;

        var systemPrompt = new PromptBuilder()
            .WithPersona($"You are a professional technical interviewer conducting a mock interview. The candidate's level is {difficultyLevel}.")
            .WithRules(InterviewRules.GetRulesFor(difficultyLevel))
            .Build();

        var llmMessages = new List<LlmMessage>
        {
            new(LlmRoles.System, systemPrompt)
        };

        // Add the full conversation history so the LLM has context
        foreach (var msg in interview.Messages)
        {
            var role = msg.Role == MessageRole.Interviewer ? LlmRoles.Assistant : LlmRoles.User;
            llmMessages.Add(new LlmMessage(role, msg.Content));
        }

        // Step 4: Get follow-up question from LLM
        var followUp = await _llmClient.ChatAsync(llmMessages, cancellationToken);

        // Step 5: Add the follow-up question
        interview.AddInterviewerMessage(followUp);

        // Step 6: Save changes
        await _interviewRepository.UpdateAsync(interview, cancellationToken);

        _logger.LogInformation("Message exchange completed for interview {InterviewId}. Total messages: {Count}.",
            interview.Id, interview.Messages.Count);

        // Step 7: Return the new follow-up message
        var lastMessage = interview.Messages[^1];
        return Result<InterviewMessageDto>.Ok(lastMessage.ToDto());
    }
}
