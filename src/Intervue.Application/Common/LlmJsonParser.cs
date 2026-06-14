using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Intervue.Application.Common;

/// <summary>
/// Shared utility for parsing JSON responses from the LLM.
/// Handles common LLM quirks: markdown code fences, snake_case fields,
/// quoted numbers, and trailing commas.
/// </summary>
public static class LlmJsonParser
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Common snake_case → camelCase replacements that LLMs frequently produce.
    /// </summary>
    private static readonly (string From, string To)[] SnakeCaseReplacements =
    [
        ("years_of_experience", "yearsOfExperience"),
        ("difficulty_level", "difficultyLevel"),
        ("duration_months", "durationMonths"),
        ("technologies_used", "technologiesUsed"),
        ("overall_score", "overallScore"),
        ("category_scores", "categoryScores")
    ];

    /// <summary>
    /// Attempts to deserialize an LLM response string into <typeparamref name="T"/>.
    /// Handles markdown fences, snake_case normalization, and quoted numbers.
    /// Returns null if parsing fails.
    /// </summary>
    public static T? TryParse<T>(string llmResponse, ILogger? logger = null) where T : class
    {
        try
        {
            var json = StripMarkdownFences(llmResponse);
            json = ExtractJsonObject(json);

            if (json is null)
            {
                logger?.LogWarning("No JSON object found in LLM response.");
                return null;
            }

            json = NormalizeSnakeCase(json);
            json = FixQuotedNumbers(json);

            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            logger?.LogWarning(ex, "LLM response JSON parsing failed.");
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Unexpected error while parsing LLM response.");
            return null;
        }
    }

    /// <summary>
    /// Strips markdown code fences (```json ... ``` or ``` ... ```) from the response.
    /// </summary>
    private static string StripMarkdownFences(string text)
    {
        // Match ```json ... ``` or ``` ... ```
        var fenceMatch = Regex.Match(text, @"```(?:json)?\s*\n?(.*?)\n?\s*```", RegexOptions.Singleline);
        return fenceMatch.Success ? fenceMatch.Groups[1].Value : text;
    }

    /// <summary>
    /// Extracts the outermost JSON object ({ ... }) from the text.
    /// </summary>
    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');

        if (start < 0 || end <= start)
            return null;

        return text.Substring(start, end - start + 1);
    }

    /// <summary>
    /// Normalizes common snake_case field names to camelCase.
    /// </summary>
    private static string NormalizeSnakeCase(string json)
    {
        foreach (var (from, to) in SnakeCaseReplacements)
        {
            json = json.Replace(from, to);
        }

        return json;
    }

    /// <summary>
    /// Fixes quoted numbers for known numeric fields: "yearsOfExperience": "3" → "yearsOfExperience": 3
    /// </summary>
    private static string FixQuotedNumbers(string json)
    {
        return Regex.Replace(json, @"""(yearsOfExperience|durationMonths|overallScore|score)"":\s*""(\d+)""", "\"$1\": $2");
    }
}
