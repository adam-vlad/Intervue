namespace Intervue.Application.Common.Prompts;

/// <summary>
/// Named <see cref="PromptRule"/> constants used when parsing CV text with the LLM.
/// These rules instruct the model to return structured JSON from raw CV text.
/// </summary>
public static class CvParsingRules
{
    /// <summary>Return only valid JSON — no markdown, no explanation.</summary>
    public static readonly PromptRule ReturnOnlyJson = new(
        "Return ONLY a valid JSON object. No markdown, no explanation, no text before or after.");

    /// <summary>Estimate years of experience from context when not explicitly stated.</summary>
    public static readonly PromptRule EstimateExperience = new(
        "Estimate yearsOfExperience based on context if not explicitly stated (minimum 1).");

    /// <summary>Estimate duration in months from date ranges.</summary>
    public static readonly PromptRule EstimateDuration = new(
        "Estimate durationMonths from dates if available.");

    /// <summary>Determine difficulty level from total professional work experience only (sum of durationMonths / 12).</summary>
    public static readonly PromptRule DetermineDifficulty = new(
        "Set difficultyLevel based ONLY on total professional work experience. " +
        "Calculate this as: sum of all durationMonths from the experiences list, divided by 12. " +
        "Do NOT use yearsOfExperience from the technologies list for this calculation. " +
        "Under 2 years total = Junior. From 2 to 5 years = Mid. Over 5 years = Senior. " +
        "If the experiences list is empty or all durationMonths are 0, set difficultyLevel to Junior.");

    /// <summary>Use the exact JSON schema provided.</summary>
    public static readonly PromptRule UseExactSchema = new(
        "Use the exact JSON property names: difficultyLevel, education, technologies (with name, yearsOfExperience), experiences (with role, company, durationMonths, description), projects (with name, description, technologiesUsed).");

    /// <summary>
    /// Dynamic rule that combines date context with duration estimation.
    /// Replaces <see cref="EstimateDuration"/> in production — the LLM needs today's date
    /// to correctly calculate durationMonths for jobs still marked as 'Present'.
    /// </summary>
    public static PromptRule EstimateDurationWithDate(DateOnly today) => new(
        $"Today's date is {today:MMMM yyyy}. Calculate durationMonths from date ranges. " +
        $"For entries where the end date is 'Present', 'current', 'ongoing', 'till date', 'prezent', " +
        $"or any equivalent meaning the role is still active, use today as the end date.");

    /// <summary>Returns all CV parsing rules in the standard order. Used in tests.</summary>
    public static IReadOnlyList<PromptRule> All => new[]
    {
        ReturnOnlyJson,
        UseExactSchema,
        EstimateExperience,
        EstimateDuration,
        DetermineDifficulty
    };

    /// <summary>
    /// Returns all CV parsing rules with today's date injected into the duration rule.
    /// Use this in production — ensures correct durationMonths for jobs still active ('Present').
    /// </summary>
    public static IReadOnlyList<PromptRule> AllWithDate(DateOnly today) => new[]
    {
        ReturnOnlyJson,
        UseExactSchema,
        EstimateExperience,
        EstimateDurationWithDate(today),
        DetermineDifficulty
    };
}
