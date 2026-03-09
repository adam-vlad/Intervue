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

    /// <summary>Determine difficulty level from total experience.</summary>
    public static readonly PromptRule DetermineDifficulty = new(
        "Set difficultyLevel based on total experience: <2 years = Junior, 2-5 years = Mid, >5 years = Senior.");

    /// <summary>Use the exact JSON schema provided.</summary>
    public static readonly PromptRule UseExactSchema = new(
        "Use the exact JSON property names: difficultyLevel, education, technologies (with name, yearsOfExperience), experiences (with role, company, durationMonths, description), projects (with name, description, technologiesUsed).");

    /// <summary>Returns all CV parsing rules in the standard order.</summary>
    public static IReadOnlyList<PromptRule> All => new[]
    {
        ReturnOnlyJson,
        UseExactSchema,
        EstimateExperience,
        EstimateDuration,
        DetermineDifficulty
    };
}
