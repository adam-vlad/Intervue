namespace MockInterview.Infrastructure.Configuration;

/// <summary>
/// Settings for connecting to the Ollama LLM server.
/// Bound from appsettings.json section "Ollama".
/// </summary>
public class OllamaSettings
{
    public const string SectionName = "Ollama";

    /// <summary>Base URL of the Ollama server (e.g., http://localhost:11434).</summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>Model name to use (e.g., llama3:8b-instruct-q4_0).</summary>
    public string Model { get; set; } = "llama3:8b-instruct-q4_0";
}
