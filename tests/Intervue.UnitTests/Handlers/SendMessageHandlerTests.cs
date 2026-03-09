using FluentAssertions;
using Moq;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Features.DTOs;
using Intervue.Application.Features.Interview.SendMessage;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for SendMessageHandler.
/// Mocks: IInterviewRepository, ICvProfileRepository, ILlmClient.
/// </summary>
public class SendMessageHandlerTests
{
    private readonly Mock<IInterviewRepository> _interviewRepository = new();
    private readonly Mock<ICvProfileRepository> _cvProfileRepository = new();
    private readonly Mock<ILlmClient> _llmClient = new();
    private readonly SendMessageHandler _sut;

    public SendMessageHandlerTests()
    {
        _sut = new SendMessageHandler(
            _interviewRepository.Object,
            _cvProfileRepository.Object,
            _llmClient.Object);
    }

    [Fact]
    public async Task Handle_WithValidMessage_ReturnsFollowUpQuestion()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var cvProfileId = Guid.NewGuid();
        var command = new SendMessageCommand(interviewId, "My answer about DI patterns");

        var interview = CreateStartedInterview(cvProfileId);
        var cvProfile = CreateCvProfile();

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);
        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);
        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Great answer! Can you explain SOLID principles?");
        _interviewRepository.Setup(x => x.UpdateAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Role.Should().Be(MessageRole.Interviewer);
        result.Value.Content.Should().Contain("SOLID");

        _interviewRepository.Verify(x => x.UpdateAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInterviewNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new SendMessageCommand(Guid.NewGuid(), "Some answer");

        _interviewRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Interview?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Interview.NotFound");

        _llmClient.Verify(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SendsFullConversationHistoryToLlm()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var cvProfileId = Guid.NewGuid();
        var command = new SendMessageCommand(interviewId, "My answer");

        var interview = CreateStartedInterview(cvProfileId);
        var cvProfile = CreateCvProfile();

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);
        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        IReadOnlyList<LlmMessage>? capturedMessages = null;
        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<LlmMessage>, CancellationToken>((msgs, _) => capturedMessages = msgs)
            .ReturnsAsync("Follow-up");

        _interviewRepository.Setup(x => x.UpdateAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        capturedMessages.Should().NotBeNull();
        // system + initial interviewer message + candidate message
        capturedMessages!.Count.Should().BeGreaterThanOrEqualTo(3);
        capturedMessages[0].Role.Should().Be("system");
        capturedMessages[0].Content.Should().Contain("Rules:");
    }

    [Fact]
    public async Task Handle_WhenCvProfileNotFound_DefaultsToJuniorLevel()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var cvProfileId = Guid.NewGuid();
        var command = new SendMessageCommand(interviewId, "My answer");

        var interview = CreateStartedInterview(cvProfileId);

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);
        _cvProfileRepository.Setup(x => x.GetByIdAsync(cvProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CvProfile?)null);

        IReadOnlyList<LlmMessage>? capturedMessages = null;
        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<LlmMessage>, CancellationToken>((msgs, _) => capturedMessages = msgs)
            .ReturnsAsync("Follow-up question");

        _interviewRepository.Setup(x => x.UpdateAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedMessages![0].Content.Should().Contain("Junior");
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static Interview CreateStartedInterview(Guid cvProfileId)
    {
        var interview = Interview.Create(cvProfileId);
        interview.Start("What is dependency injection?");
        return interview;
    }

    private static CvProfile CreateCvProfile()
    {
        var cvProfile = CvProfile.Create("CV text", new HashedPersonalData("hash"));
        cvProfile.SetParsedData(
            DifficultyLevel.Junior,
            "BSc CS",
            new List<Technology> { Technology.Create("C#", 2) },
            new List<Experience>(),
            new List<Project>());
        return cvProfile;
    }
}
