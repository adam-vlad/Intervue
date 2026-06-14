using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Features.Cv.ParseCv;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for ParseCvHandler.
/// Mocks: ICvProfileRepository, ILlmClient, ILogger.
/// </summary>
public class ParseCvHandlerTests
{
    private readonly Mock<ICvProfileRepository> _cvProfileRepository = new();
    private readonly Mock<ILlmClient> _llmClient = new();
    private readonly Mock<ILogger<ParseCvHandler>> _logger = new();
    private readonly ParseCvHandler _sut;

    public ParseCvHandlerTests()
    {
        _sut = new ParseCvHandler(
            _cvProfileRepository.Object,
            _llmClient.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCvProfile_ReturnsParsedDto()
    {
        // Arrange
        var cvProfileId = Guid.NewGuid();
        var command = new ParseCvCommand(cvProfileId);
        var cvProfile = CvProfile.Create("John Doe\nC# Developer\n5 years", new HashedPersonalData("hash123"));

        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        var llmResponse = """
            {
                "difficultyLevel": "Mid",
                "education": "BSc Computer Science",
                "technologies": [
                    { "name": "C#", "yearsOfExperience": 5 }
                ],
                "experiences": [
                    { "role": "Developer", "company": "Acme", "durationMonths": 24, "description": "Backend dev" }
                ],
                "projects": [
                    { "name": "API Gateway", "description": "REST API", "technologiesUsed": ["C#", "ASP.NET"] }
                ]
            }
            """;

        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(llmResponse);

        _cvProfileRepository.Setup(x => x.UpdateAsync(It.IsAny<CvProfile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.DifficultyLevel.Should().Be(DifficultyLevel.Mid);
        result.Value.Technologies.Should().ContainSingle(t => t.Name == "C#");

        _llmClient.Verify(x => x.ChatAsync(
            It.Is<IReadOnlyList<LlmMessage>>(m => m[0].Role == "system"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCvProfileNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var cvProfileId = Guid.NewGuid();
        var command = new ParseCvCommand(cvProfileId);

        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CvProfile?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Cv.NotFound");

        _llmClient.Verify(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenLlmReturnsInvalidJson_ReturnsParseFailedError()
    {
        // Arrange
        var cvProfileId = Guid.NewGuid();
        var command = new ParseCvCommand(cvProfileId);
        var cvProfile = CvProfile.Create("Some raw text", new HashedPersonalData("hash"));

        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("This is not valid JSON at all");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Cv.ParseFailed");
    }

    [Fact]
    public async Task Handle_WhenLlmReturnsMarkdownWrappedJson_ParsesSuccessfully()
    {
        // Arrange
        var cvProfileId = Guid.NewGuid();
        var command = new ParseCvCommand(cvProfileId);
        var cvProfile = CvProfile.Create("Developer CV", new HashedPersonalData("hash"));

        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        var llmResponse = """
            ```json
            {
                "difficultyLevel": "Junior",
                "education": null,
                "technologies": [{ "name": "Python", "yearsOfExperience": 1 }],
                "experiences": [],
                "projects": []
            }
            ```
            """;

        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(llmResponse);
        _cvProfileRepository.Setup(x => x.UpdateAsync(It.IsAny<CvProfile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.DifficultyLevel.Should().Be(DifficultyLevel.Junior);
    }

    [Fact]
    public async Task Handle_SystemPromptContainsRules()
    {
        // Arrange
        var cvProfileId = Guid.NewGuid();
        var command = new ParseCvCommand(cvProfileId);
        var cvProfile = CvProfile.Create("CV text", new HashedPersonalData("hash"));

        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        IReadOnlyList<LlmMessage>? capturedMessages = null;
        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<LlmMessage>, CancellationToken>((msgs, _) => capturedMessages = msgs)
            .ReturnsAsync("not valid");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        capturedMessages.Should().NotBeNull();
        capturedMessages![0].Role.Should().Be("system");
        capturedMessages[0].Content.Should().Contain("Rules:");
        capturedMessages[0].Content.Should().Contain("Return ONLY a valid JSON");
    }
}
