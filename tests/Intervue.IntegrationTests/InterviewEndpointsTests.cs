using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using Intervue.Application.Common.Interfaces;

namespace Intervue.IntegrationTests;

/// <summary>
/// Integration tests for the Interview endpoints (start, message, feedback, get).
/// Uses WebApplicationFactory with InMemory DB and mocked LLM.
/// </summary>
public class InterviewEndpointsTests : IClassFixture<IntervueWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IntervueWebApplicationFactory _factory;

    public InterviewEndpointsTests(IntervueWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Start_WithNonExistentCvProfileId_Returns404NotFound()
    {
        // Arrange
        var command = new { CvProfileId = Guid.NewGuid() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/interview/start", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Start_WithEmptyCvProfileId_Returns400BadRequest()
    {
        // Arrange
        var command = new { CvProfileId = Guid.Empty };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/interview/start", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendMessage_WithNonExistentInterviewId_Returns404NotFound()
    {
        // Arrange
        var command = new { InterviewId = Guid.NewGuid(), Content = "My answer" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/interview/message", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SendMessage_WithEmptyContent_Returns400BadRequest()
    {
        // Arrange
        var command = new { InterviewId = Guid.NewGuid(), Content = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/interview/message", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateFeedback_WithNonExistentInterviewId_Returns404NotFound()
    {
        // Arrange
        var command = new { InterviewId = Guid.NewGuid() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/interview/feedback", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GenerateFeedback_WithEmptyInterviewId_Returns400BadRequest()
    {
        // Arrange
        var command = new { InterviewId = Guid.Empty };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/interview/feedback", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetInterview_WithNonExistentId_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/interview/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetInterview_WithEmptyId_Returns404OrBadRequest()
    {
        // Act  
        var response = await _client.GetAsync($"/api/v1/interview/{Guid.Empty}");

        // Assert — either 400 or 404 is acceptable (validation pipeline catches empty Guid)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }
}
