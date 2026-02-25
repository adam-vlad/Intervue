namespace MockInterview.Application.Features.DTOs;

/// <summary>
/// Data Transfer Object for the feedback report generated at the end of an interview.
/// </summary>
public record FeedbackReportDto(
    Guid Id,
    int OverallScore,
    List<InterviewScoreDto> CategoryScores,
    string Strengths,
    string Weaknesses,
    string Suggestions,
    DateTime GeneratedAt);

public record InterviewScoreDto(string Category, int Score);
