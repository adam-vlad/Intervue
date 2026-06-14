namespace Intervue.Application.Common.Constants;

/// <summary>
/// Constants for LLM message roles used when communicating with Ollama.
/// Prevents magic strings scattered across handlers.
/// </summary>
public static class LlmRoles
{
    public const string System = "system";
    public const string User = "user";
    public const string Assistant = "assistant";
}
