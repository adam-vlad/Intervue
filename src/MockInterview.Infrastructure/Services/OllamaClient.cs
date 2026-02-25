using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MockInterview.Application.Common.Interfaces;
using MockInterview.Infrastructure.Configuration;

namespace MockInterview.Infrastructure.Services;

/// <summary>
/// Implements ILlmClient by sending HTTP POST requests to Ollama's /api/chat endpoint.
/// Ollama runs in a Docker container and exposes a REST API.
/// </summary>
public class OllamaClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _settings;

    public OllamaClient(HttpClient httpClient, IOptions<OllamaSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
    }

    public async Task<string> ChatAsync(
        IReadOnlyList<LlmMessage> messages,
        CancellationToken cancellationToken = default)
    {
        // Build the request body that Ollama expects
        var requestBody = new OllamaChatRequest
        {
            Model = _settings.Model,
            Messages = messages.Select(m => new OllamaChatMessage
            {
                Role = m.Role,
                Content = m.Content
            }).ToList(),
            Stream = false // We want the full response at once, not streamed
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/chat", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        var chatResponse = JsonSerializer.Deserialize<OllamaChatResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return chatResponse?.Message?.Content ?? string.Empty;
    }

    // ── Internal DTOs for Ollama API ────────────────────────────────

    private class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<OllamaChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    private class OllamaChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaChatMessage? Message { get; set; }
    }
}
