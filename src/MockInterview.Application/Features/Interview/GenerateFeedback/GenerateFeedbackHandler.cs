using System.Text.Json;
using MediatR;
using MockInterview.Application.Common;
using MockInterview.Application.Common.Interfaces;
using MockInterview.Application.Features.DTOs;
using MockInterview.Domain.Entities;
using MockInterview.Domain.Enums;
using MockInterview.Domain.Repositories;
using MockInterview.Domain.ValueObjects;

namespace MockInterview.Application.Features.Interview.GenerateFeedback;

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

    public GenerateFeedbackHandler(IInterviewRepository interviewRepository, ILlmClient llmClient)
    {
        _interviewRepository = interviewRepository;
        _llmClient = llmClient;
    }

    public async Task<Result<FeedbackReportDto>> Handle(GenerateFeedbackCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Get the interview
        var interview = await _interviewRepository.GetByIdAsync(request.InterviewId, cancellationToken);

        if (interview is null)
        {
            return Result<FeedbackReportDto>.Fail(
                Error.NotFound("Interview.NotFound", $"Interview with id '{request.InterviewId}' was not found."));
        }

        if (interview.Status != InterviewStatus.InProgress)
        {
            return Result<FeedbackReportDto>.Fail(
                Error.Conflict("Interview.NotInProgress", $"Interview is '{interview.Status}', must be InProgress to generate feedback."));
        }

        // Step 2: Build the conversation transcript for the LLM
        var transcript = string.Join("\n\n", interview.Messages.Select(m =>
            $"{(m.Role == MessageRole.Interviewer ? "Interviewer" : "Candidate")}: {m.Content}"));

        var messages = new List<LlmMessage>
        {
            new("system", FeedbackPrompt),
            new("user", $"Here is the interview transcript:\n\n{transcript}")
        };

        // Step 3: Get feedback from LLM
        var llmResponse = await _llmClient.ChatAsync(messages, cancellationToken);

        // Step 4: Parse the feedback JSON
        var parsedFeedback = ParseFeedbackResponse(llmResponse);

        if (parsedFeedback is null)
        {
            return Result<FeedbackReportDto>.Fail(
                Error.Failure("Feedback.ParseFailed", "Failed to parse the LLM's feedback response."));
        }

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

    private static ParsedFeedback? ParseFeedbackResponse(string llmResponse)
    {
        try
        {
            var json = llmResponse;

            // Strip markdown code fences (```json ... ``` or ``` ... ```)
            json = System.Text.RegularExpressions.Regex.Replace(json, @"```(?:json)?\s*", "");

            // Extract outermost JSON object
            var jsonStart = json.IndexOf('{');
            var jsonEnd = json.LastIndexOf('}');

            if (jsonStart < 0 || jsonEnd <= jsonStart)
                return null;

            json = json.Substring(jsonStart, jsonEnd - jsonStart + 1);

            // Normalize common LLM variations: snake_case → camelCase
            json = json.Replace("overall_score", "overallScore")
                       .Replace("category_scores", "categoryScores");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            var result = JsonSerializer.Deserialize<ParsedFeedback>(json, options);

            // Fallback: if categoryScores is empty, generate defaults
            if (result is not null && result.CategoryScores.Count == 0)
            {
                result.CategoryScores = new List<ParsedCategoryScore>
                {
                    new() { Category = "Technical Knowledge", Score = result.OverallScore },
                    new() { Category = "Problem Solving", Score = result.OverallScore },
                    new() { Category = "Communication", Score = result.OverallScore },
                    new() { Category = "Experience Relevance", Score = result.OverallScore }
                };
            }

            // Ensure strings are not empty
            if (result is not null)
            {
                if (string.IsNullOrWhiteSpace(result.Strengths)) result.Strengths = "Not evaluated.";
                if (string.IsNullOrWhiteSpace(result.Weaknesses)) result.Weaknesses = "Not evaluated.";
                if (string.IsNullOrWhiteSpace(result.Suggestions)) result.Suggestions = "No suggestions.";
            }

            return result;
        }
        catch
        {
            return null;
        }
    }

    private const string FeedbackPrompt = """
        You are an expert interview evaluator. Analyze the interview transcript and provide a detailed feedback report.
        
        You MUST respond with ONLY a valid JSON object. No markdown, no explanation, no text before or after.
        Use this EXACT structure with these EXACT property names:
        
        {"overallScore": 75, "categoryScores": [{"category": "Technical Knowledge", "score": 70}, {"category": "Problem Solving", "score": 75}, {"category": "Communication", "score": 80}, {"category": "Experience Relevance", "score": 72}], "strengths": "text here", "weaknesses": "text here", "suggestions": "text here"}
        
        Property rules:
        - overallScore: integer 0-100
        - categoryScores: array with exactly 4 objects, each with "category" (string) and "score" (integer 0-100)
        - strengths: 2-3 sentences about what the candidate did well
        - weaknesses: 2-3 sentences about areas for improvement  
        - suggestions: 2-3 concrete suggestions for the candidate
        
        IMPORTANT: Respond ONLY with the JSON object. No other text at all.
        """;

    private class ParsedFeedback
    {
        public int OverallScore { get; set; }
        public List<ParsedCategoryScore> CategoryScores { get; set; } = new();
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string Suggestions { get; set; } = string.Empty;
    }

    private class ParsedCategoryScore
    {
        public string Category { get; set; } = string.Empty;
        public int Score { get; set; }
    }
}
