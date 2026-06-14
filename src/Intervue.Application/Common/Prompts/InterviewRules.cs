using Intervue.Domain.Enums;

namespace Intervue.Application.Common.Prompts;

/// <summary>
/// Named <see cref="PromptRule"/> constants used during interview conversation.
/// Provides difficulty-specific rules via <see cref="GetRulesFor(DifficultyLevel)"/>.
/// </summary>
public static class InterviewRules
{
    // ── Common rules (all levels) ───────────────────────────────────

    /// <summary>Ask focused follow-up questions based on the candidate's answers.</summary>
    public static readonly PromptRule FollowUp = new(
        "Ask focused, follow-up questions based on the candidate's answers.");

    /// <summary>If the answer is vague, press for specific examples or deeper explanation.</summary>
    public static readonly PromptRule PressForExamples = new(
        "If the answer is vague, ask for specific examples or deeper explanation.");

    /// <summary>If the answer is good, move to a related but different topic.</summary>
    public static readonly PromptRule MoveOnIfGood = new(
        "If the answer is good, move to a related but different topic.");

    /// <summary>Ask only one question at a time.</summary>
    public static readonly PromptRule OneQuestionAtATime = new(
        "Ask only one question at a time. Keep questions concise and clear.");

    /// <summary>Respond with only the next question — no commentary.</summary>
    public static readonly PromptRule OutputFormatQuestionOnly = new(
        "Respond with ONLY your next question, nothing else.");

    /// <summary>Be professional and encouraging.</summary>
    public static readonly PromptRule ProfessionalTone = new(
        "Be professional and encouraging.");

    // ── Junior-specific rules ───────────────────────────────────────

    /// <summary>Give hints when the candidate is stuck.</summary>
    public static readonly PromptRule GiveHints = new(
        "If the candidate seems stuck, provide a small hint or rephrase the question to guide them.");

    /// <summary>Focus on fundamentals and basic concepts.</summary>
    public static readonly PromptRule FocusOnFundamentals = new(
        "Focus on fundamentals, basic concepts, and practical usage rather than advanced theory.");

    /// <summary>Use simple, beginner-friendly language.</summary>
    public static readonly PromptRule BeginnerFriendlyLanguage = new(
        "Use simple, beginner-friendly language. Avoid overly complex terminology.");

    // ── Mid-specific rules ──────────────────────────────────────────

    /// <summary>Ask about trade-offs and design decisions.</summary>
    public static readonly PromptRule AskAboutTradeOffs = new(
        "Ask about trade-offs and design decisions the candidate has made in real projects.");

    /// <summary>Explore depth of understanding beyond surface-level answers.</summary>
    public static readonly PromptRule ExploreDepth = new(
        "Explore depth of understanding — go beyond surface-level answers into implementation details.");

    // ── Senior-specific rules ───────────────────────────────────────

    /// <summary>Include system design questions.</summary>
    public static readonly PromptRule SystemDesign = new(
        "Include system design questions — ask about architecture, scalability, and distributed systems.");

    /// <summary>Ask about edge cases and failure scenarios.</summary>
    public static readonly PromptRule EdgeCases = new(
        "Ask about edge cases, failure scenarios, and how the candidate handles production incidents.");

    /// <summary>Expect the candidate to justify decisions with concrete reasoning.</summary>
    public static readonly PromptRule JustifyDecisions = new(
        "Expect the candidate to justify technical decisions with concrete reasoning and real-world experience.");

    /// <summary>
    /// Returns the appropriate set of rules for the given difficulty level.
    /// Junior gets hint-friendly rules, Senior gets system-design and edge-case rules.
    /// </summary>
    public static IReadOnlyList<PromptRule> GetRulesFor(DifficultyLevel level)
    {
        var rules = new List<PromptRule>
        {
            FollowUp,
            PressForExamples,
            MoveOnIfGood,
            OneQuestionAtATime,
            ProfessionalTone,
            OutputFormatQuestionOnly
        };

        switch (level)
        {
            case DifficultyLevel.Junior:
                rules.Add(GiveHints);
                rules.Add(FocusOnFundamentals);
                rules.Add(BeginnerFriendlyLanguage);
                break;

            case DifficultyLevel.Mid:
                rules.Add(AskAboutTradeOffs);
                rules.Add(ExploreDepth);
                break;

            case DifficultyLevel.Senior:
                rules.Add(SystemDesign);
                rules.Add(EdgeCases);
                rules.Add(JustifyDecisions);
                break;
        }

        return rules.AsReadOnly();
    }
}
