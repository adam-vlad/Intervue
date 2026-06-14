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

    /// <summary>Score unevaluated categories as 50 (neutral), never 0 unless total failure was demonstrated.</summary>
    public static readonly PromptRule NeutralUnevaluated = new(
        "If a category cannot be properly evaluated because the conversation did not cover it or was too short, " +
        "assign a score of 50 (neutral/unknown). Reserve 0 ONLY for cases where the candidate demonstrated " +
        "clear and complete failure or total ignorance on a specific topic they were directly asked about.");

    /// <summary>Communication score reflects quality of written responses, not technical depth.</summary>
    public static readonly PromptRule CommunicationDefinition = new(
        "Communication score evaluates the clarity, coherence, and professionalism of the candidate's written responses. " +
        "It is independent of whether technical topics were covered. " +
        "If the candidate sent at least one message, the minimum Communication score is 35.");

    /// <summary>Dynamic rule: calibrate scoring expectations to the candidate's declared difficulty level.</summary>
    public static PromptRule CalibrateToDifficulty(string difficulty) => new(
        $"The candidate is a {difficulty}-level developer. Calibrate your expectations accordingly. " +
        $"A Junior is not expected to demonstrate Senior-level depth. " +
        $"Evaluate relative to what is reasonable for their declared level.");

    /// <summary>Dynamic rule: provide context about interview length to guide proportional scoring.</summary>
    public static PromptRule MessageCountContext(int candidateMessageCount) => new(
        candidateMessageCount < 4
            ? $"The candidate sent only {candidateMessageCount} response(s). This is a short interview. " +
              "Score categories proportionally — do not penalize for topics that were simply not discussed."
            : $"The candidate sent {candidateMessageCount} responses. Evaluate all categories based on what was demonstrated.");

    /// <summary>Returns all static feedback rules in the standard order.</summary>
    public static IReadOnlyList<PromptRule> All => new[]
    {
        ReturnOnlyJson,
        UseExactSchema,
        ScoreRange,
        FourCategories,
        StrengthsFormat,
        WeaknessesFormat,
        SuggestionsFormat,
        NeutralUnevaluated,
        CommunicationDefinition
    };
}
