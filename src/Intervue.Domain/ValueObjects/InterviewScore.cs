using Intervue.Domain.Common;

namespace Intervue.Domain.ValueObjects;

/// <summary>
/// Value Object representing a score for a specific interview category (e.g., "C# knowledge: 85/100").
/// </summary>
public sealed record InterviewScore
{
    /// <summary>Category name, e.g., "Technical Knowledge", "Communication".</summary>
    public string Category { get; }

    /// <summary>Score from 0 to 100.</summary>
    public int Score { get; }

    public InterviewScore(string category, int score)
    {
        Category = Guard.AgainstNullOrWhiteSpace(category, nameof(category));
        Score = Guard.InRange(score, 0, 100, nameof(score));
    }
}
