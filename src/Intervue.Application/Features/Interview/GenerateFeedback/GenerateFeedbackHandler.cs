using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common;
using Intervue.Application.Common.Constants;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Common.Prompts;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.Application.Features.Interview.GenerateFeedback;

/// <summary>
/// Handles GenerateFeedbackCommand:
/// 1. Gets the Interview from repository
/// 2. Sends the full conversation to the LLM for analysis
/// 3. Parses the feedback JSON from the LLM
/// 4. Creates a FeedbackReport entity
/// 5. Completes the interview
/// 6. Returns FeedbackReportDto
/// </summary>
public class GenerateFeedbackHandler : IRequestHandler<GenerateFeedbackCommand, Result<FeedbackReportDto>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly ILlmClient _llmClient;
    private readonly ILogger<GenerateFeedbackHandler> _logger;

    public GenerateFeedbackHandler(
        IInterviewRepository interviewRepository,
        ILlmClient llmClient,
        ILogger<GenerateFeedbackHandler> logger)
    {
        _interviewRepository = interviewRepository;
        _llmClient = llmClient;
        _logger = logger;
    }

    public async Task<Result<FeedbackReportDto>> Handle(GenerateFeedbackCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Get the interview
        var interview = await _interviewRepository.GetByIdAsync(request.InterviewId, cancellationToken);

        if (interview is null)
        {
            return Result<FeedbackReportDto>.Fail(
                Error.NotFound(ErrorCodes.InterviewNotFound, $"Interview with id '{request.InterviewId}' was not found."));
        }

        if (interview.Status != InterviewStatus.InProgress)
        {
            return Result<FeedbackReportDto>.Fail(
                Error.Conflict(ErrorCodes.InterviewNotInProgress, $"Interview is '{interview.Status}', must be InProgress to generate feedback."));
        }

        // Step 2: Build the conversation transcript for the LLM
        var transcript = string.Join("\n\n", interview.Messages.Select(m =>
            $"{(m.Role == MessageRole.Interviewer ? "Interviewer" : "Candidate")}: {m.Content}"));

        var systemPrompt = new PromptBuilder()
            .WithPersona("You are an expert interview evaluator. Analyze the interview transcript and provide a detailed feedback report.")
            .WithRules(FeedbackRules.All)
            .Build();

        var messages = new List<LlmMessage>
        {
            new(LlmRoles.System, systemPrompt),
            new(LlmRoles.User, $"Here is the interview transcript:\n\n{transcript}")
        };

        // Step 3: Get feedback from LLM
        var llmResponse = await _llmClient.ChatAsync(messages, cancellationToken);

        // Step 4: Parse the feedback JSON
        _logger.LogInformation("Raw LLM response for feedback:\n{LlmResponse}", llmResponse);

        var parsedFeedback = LlmJsonParser.TryParse<ParsedFeedback>(llmResponse, _logger);

        if (parsedFeedback is null)
        {
            return Result<FeedbackReportDto>.Fail(
                Error.Failure(ErrorCodes.FeedbackParseFailed, "Failed to parse the LLM's feedback response."));
        }

        // Fallback: if categoryScores is empty, generate defaults
        if (parsedFeedback.CategoryScores.Count == 0)
        {
            parsedFeedback.CategoryScores = new List<ParsedCategoryScore>
            {
                new() { Category = "Technical Knowledge", Score = parsedFeedback.OverallScore },
                new() { Category = "Problem Solving", Score = parsedFeedback.OverallScore },
                new() { Category = "Communication", Score = parsedFeedback.OverallScore },
                new() { Category = "Experience Relevance", Score = parsedFeedback.OverallScore }
            };
        }

        // Ensure strings are not empty
        if (string.IsNullOrWhiteSpace(parsedFeedback.Strengths)) parsedFeedback.Strengths = "Not evaluated.";
        if (string.IsNullOrWhiteSpace(parsedFeedback.Weaknesses)) parsedFeedback.Weaknesses = "Not evaluated.";
        if (string.IsNullOrWhiteSpace(parsedFeedback.Suggestions)) parsedFeedback.Suggestions = "No suggestions.";

        // Step 5: Create domain entities
        var categoryScores = parsedFeedback.CategoryScores
            .Select(s => new InterviewScore(s.Category, Math.Clamp(s.Score, 0, 100)))
            .ToList();

        var feedbackReport = FeedbackReport.Create(
            Math.Clamp(parsedFeedback.OverallScore, 0, 100),
            categoryScores,
            parsedFeedback.Strengths,
            parsedFeedback.Weaknesses,
            parsedFeedback.Suggestions);

        // Step 6: Complete the interview
        interview.Complete(feedbackReport);

        // Step 7: Save
        await _interviewRepository.UpdateAsync(interview, cancellationToken);

        // Step 8: Return the DTO
        return Result<FeedbackReportDto>.Ok(feedbackReport.ToDto());
    }

    // ── Internal DTOs for deserializing the LLM's feedback response ───

    internal class ParsedFeedback
    {
        public int OverallScore { get; set; }
        public List<ParsedCategoryScore> CategoryScores { get; set; } = new();
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string Suggestions { get; set; } = string.Empty;
    }

    internal class ParsedCategoryScore
    {
        public string Category { get; set; } = string.Empty;
        public int Score { get; set; }
    }
}
