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
/// 2. Loads the CV profile for calibration context (difficulty level, technologies)
/// 3. Sends the full conversation to the LLM for analysis
/// 4. Parses the feedback JSON from the LLM
/// 5. Creates a FeedbackReport entity
/// 6. Completes the interview
/// 7. Returns FeedbackReportDto
/// </summary>
public class GenerateFeedbackHandler : IRequestHandler<GenerateFeedbackCommand, Result<FeedbackReportDto>>
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly ICvProfileRepository _cvProfileRepository;
    private readonly ILlmClient _llmClient;
    private readonly ILogger<GenerateFeedbackHandler> _logger;

    public GenerateFeedbackHandler(
        IInterviewRepository interviewRepository,
        ICvProfileRepository cvProfileRepository,
        ILlmClient llmClient,
        ILogger<GenerateFeedbackHandler> logger)
    {
        _interviewRepository = interviewRepository;
        _cvProfileRepository = cvProfileRepository;
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

        // Step 2: Load CV profile for calibration context (null-safe — feedback proceeds even without it)
        var cvProfile = await _cvProfileRepository.GetByIdAsync(interview.CvProfileId, cancellationToken);

        // Step 3: Build the conversation transcript for the LLM
        var transcript = string.Join("\n\n", interview.Messages.Select(m =>
            $"{(m.Role == MessageRole.Interviewer ? "Interviewer" : "Candidate")}: {m.Content}"));

        // Step 4: Compose rules — static rules + dynamic calibration rules
        var candidateMessageCount = interview.Messages.Count(m => m.Role == MessageRole.Candidate);

        var rules = new List<PromptRule>(FeedbackRules.All);
        rules.Add(FeedbackRules.MessageCountContext(candidateMessageCount));
        if (cvProfile is not null)
            rules.Add(FeedbackRules.CalibrateToDifficulty(cvProfile.DifficultyLevel.ToString()));

        var systemPrompt = new PromptBuilder()
            .WithPersona("You are an expert interview evaluator. Analyze the interview transcript and provide a detailed feedback report.")
            .WithRules(rules)
            .Build();

        // Step 5: Build user message — include CV context so the LLM can calibrate its evaluation
        var techSummary = cvProfile?.Technologies.Any() == true
            ? string.Join(", ", cvProfile.Technologies.Select(t => $"{t.Name} ({t.YearsOfExperience}y)"))
            : "not specified";

        var candidateContext = cvProfile is not null
            ? $"Candidate profile:\n- Level: {cvProfile.DifficultyLevel}\n- Technologies: {techSummary}\n- Candidate responses: {candidateMessageCount}\n\n"
            : $"Candidate responses: {candidateMessageCount}\n\n";

        var messages = new List<LlmMessage>
        {
            new(LlmRoles.System, systemPrompt),
            new(LlmRoles.User, $"{candidateContext}Interview transcript:\n\n{transcript}")
        };

        // Step 6: Get feedback from LLM
        var llmResponse = await _llmClient.ChatAsync(messages, cancellationToken);

        // Step 7: Parse the feedback JSON
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

        // Step 8: Create domain entities
        var categoryScores = parsedFeedback.CategoryScores
            .Select(s => new InterviewScore(s.Category, Math.Clamp(s.Score, 0, 100)))
            .ToList();

        var feedbackReport = FeedbackReport.Create(
            Math.Clamp(parsedFeedback.OverallScore, 0, 100),
            categoryScores,
            parsedFeedback.Strengths,
            parsedFeedback.Weaknesses,
            parsedFeedback.Suggestions);

        // Step 9: Complete the interview
        interview.Complete(feedbackReport);

        // Step 10: Save
        await _interviewRepository.UpdateAsync(interview, cancellationToken);

        // Step 11: Return the DTO
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
