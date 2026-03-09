using FluentAssertions;
using Moq;
using Intervue.Application.Common;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Features.DTOs;
using Intervue.Application.Features.Interview.StartInterview;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for StartInterviewHandler.
/// Mocks: ICvProfileRepository, IInterviewRepository, ILlmClient.
/// </summary>
public class StartInterviewHandlerTests
{
    private readonly Mock<ICvProfileRepository> _cvProfileRepository = new();
    private readonly Mock<IInterviewRepository> _interviewRepository = new();
    private readonly Mock<ILlmClient> _llmClient = new();
    private readonly StartInterviewHandler _sut;

    public StartInterviewHandlerTests()
    {
        _sut = new StartInterviewHandler(
            _cvProfileRepository.Object,
            _interviewRepository.Object,
            _llmClient.Object);
    }

    [Fact]
    public async Task Handle_WithValidCvProfile_ReturnsCreatedInterview()
    {
        // Arrange
        var cvProfileId = Guid.NewGuid();
        var command = new StartInterviewCommand(cvProfileId);
        var cvProfile = CreateCvProfileWithTechnologies(cvProfileId);

        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Hello! Tell me about your experience with C#.");

        _interviewRepository.Setup(x => x.AddAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.SuccessKind.Should().Be(SuccessKind.Created);
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(InterviewStatus.InProgress);
        result.Value.CvProfileId.Should().Be(cvProfileId);
        result.Value.Messages.Should().HaveCount(1);
        result.Value.Messages[0].Role.Should().Be(MessageRole.Interviewer);
    }

    [Fact]
    public async Task Handle_SetsPromptProfileBasedOnDifficultyLevel()
    {
        // Arrange
        var cvProfileId = Guid.NewGuid();
        var command = new StartInterviewCommand(cvProfileId);
        var cvProfile = CreateCvProfileWithTechnologies(cvProfileId, DifficultyLevel.Senior);

        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Welcome! Let's discuss system design.");

        Interview? savedInterview = null;
        _interviewRepository.Setup(x => x.AddAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .Callback<Interview, CancellationToken>((i, _) => savedInterview = i)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        savedInterview.Should().NotBeNull();
        savedInterview!.PromptProfile.Should().Be("Senior_v1");
    }

    [Fact]
    public async Task Handle_WhenCvProfileNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var cvProfileId = Guid.NewGuid();
        var command = new StartInterviewCommand(cvProfileId);

        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CvProfile?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Cv.NotFound");

        _llmClient.Verify(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
        _interviewRepository.Verify(x => x.AddAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SystemPromptUsesInterviewRules()
    {
        // Arrange
        var cvProfileId = Guid.NewGuid();
        var command = new StartInterviewCommand(cvProfileId);
        var cvProfile = CreateCvProfileWithTechnologies(cvProfileId, DifficultyLevel.Junior);

        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        IReadOnlyList<LlmMessage>? capturedMessages = null;
        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<LlmMessage>, CancellationToken>((msgs, _) => capturedMessages = msgs)
            .ReturnsAsync("First question");

        _interviewRepository.Setup(x => x.AddAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        capturedMessages.Should().NotBeNull();
        capturedMessages![0].Role.Should().Be("system");
        capturedMessages[0].Content.Should().Contain("Rules:");
        // Junior-specific rules
        capturedMessages[0].Content.Should().Contain("hint");
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static CvProfile CreateCvProfileWithTechnologies(
        Guid cvProfileId,
        DifficultyLevel level = DifficultyLevel.Junior)
    {
        var cvProfile = CvProfile.Create("Raw CV text", new HashedPersonalData("hash"));
        cvProfile.SetParsedData(
            level,
            "BSc CS",
            new List<Technology> { Technology.Create("C#", 3) },
            new List<Experience> { Experience.Create("Dev", "Acme", 12, "Backend") },
            new List<Project>());
        return cvProfile;
    }
}
