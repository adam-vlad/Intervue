using MockInterview.Domain.Common;
using MockInterview.Domain.ValueObjects;

namespace MockInterview.Domain.Entities;

/// <summary>
/// The final feedback report generated after an interview is completed.
/// Contains an overall score, per-category scores, strengths, weaknesses, and suggestions.
/// </summary>
public class FeedbackReport : Entity<Guid>
{
    public int OverallScore { get; private set; }
    public IReadOnlyList<InterviewScore> CategoryScores => _categoryScores.AsReadOnly();
    public string Strengths { get; private set; }
    public string Weaknesses { get; private set; }
    public string Suggestions { get; private set; }
    public DateTime GeneratedAt { get; private set; }

    private readonly List<InterviewScore> _categoryScores = new();

    private FeedbackReport(
        Guid id,
        int overallScore,
        List<InterviewScore> categoryScores,
        string strengths,
        string weaknesses,
        string suggestions,
        DateTime generatedAt)
        : base(id)
    {
        OverallScore = overallScore;
        _categoryScores = categoryScores;
        Strengths = strengths;
        Weaknesses = weaknesses;
        Suggestions = suggestions;
        GeneratedAt = generatedAt;
    }

    public static FeedbackReport Create(
        int overallScore,
        List<InterviewScore> categoryScores,
        string strengths,
        string weaknesses,
        string suggestions)
    {
        Guard.InRange(overallScore, 0, 100, nameof(overallScore));
        Guard.AgainstNull(categoryScores, nameof(categoryScores));
        Guard.AgainstNullOrWhiteSpace(strengths, nameof(strengths));
        Guard.AgainstNullOrWhiteSpace(weaknesses, nameof(weaknesses));
        Guard.AgainstNullOrWhiteSpace(suggestions, nameof(suggestions));

        return new FeedbackReport(
            Guid.NewGuid(),
            overallScore,
            categoryScores,
            strengths,
            weaknesses,
            suggestions,
            DateTime.UtcNow);
    }
}
