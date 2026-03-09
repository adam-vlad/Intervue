namespace Intervue.Application.Common.Prompts;

/// <summary>
/// Named <see cref="PromptRule"/> constants used when generating a feedback report from an interview transcript.
/// </summary>
public static class FeedbackRules
{
    /// <summary>Return only valid JSON — no markdown, no explanation.</summary>
    public static readonly PromptRule ReturnOnlyJson = new(
        "Respond with ONLY a valid JSON object. No markdown, no explanation, no text before or after.");

    /// <summary>Use the exact JSON schema with correct property names.</summary>
    public static readonly PromptRule UseExactSchema = new(
        "Use this EXACT structure: {\"overallScore\": number, \"categoryScores\": [{\"category\": string, \"score\": number}], \"strengths\": string, \"weaknesses\": string, \"suggestions\": string}.");

    /// <summary>Overall score must be 0-100.</summary>
    public static readonly PromptRule ScoreRange = new(
        "overallScore must be an integer between 0 and 100.");

    /// <summary>Provide exactly 4 category scores.</summary>
    public static readonly PromptRule FourCategories = new(
        "categoryScores must have exactly 4 entries: Technical Knowledge, Problem Solving, Communication, Experience Relevance — each with a score 0-100.");

    /// <summary>Strengths should be 2-3 sentences.</summary>
    public static readonly PromptRule StrengthsFormat = new(
        "strengths: 2-3 sentences about what the candidate did well.");

    /// <summary>Weaknesses should be 2-3 sentences.</summary>
    public static readonly PromptRule WeaknessesFormat = new(
        "weaknesses: 2-3 sentences about areas for improvement.");

    /// <summary>Suggestions should be 2-3 concrete suggestions.</summary>
    public static readonly PromptRule SuggestionsFormat = new(
        "suggestions: 2-3 concrete, actionable suggestions for the candidate.");

    /// <summary>Returns all feedback rules in the standard order.</summary>
    public static IReadOnlyList<PromptRule> All => new[]
    {
        ReturnOnlyJson,
        UseExactSchema,
        ScoreRange,
        FourCategories,
        StrengthsFormat,
        WeaknessesFormat,
        SuggestionsFormat
    };
}
