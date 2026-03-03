namespace Intervue.Application.Common.Interfaces;

/// <summary>
/// Contract for communicating with the LLM (Ollama).
/// Infrastructure implements this with actual HTTP calls to Ollama.
/// </summary>
public interface ILlmClient
{
    /// <summary>
    /// Sends a list of messages to the LLM and gets a text response.
    /// Messages are chat-style: [{role: "system", content: "..."}, {role: "user", content: "..."}, ...].
    /// </summary>
    Task<string> ChatAsync(
        IReadOnlyList<LlmMessage> messages,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a single message in the LLM conversation (role + content).
/// </summary>
public sealed record LlmMessage(string Role, string Content);
