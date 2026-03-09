using FluentAssertions;
using Moq;
using Intervue.Application.Common.Interfaces;
using Intervue.Application.Features.DTOs;
using Intervue.Application.Features.Interview.GenerateFeedback;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for GenerateFeedbackHandler.
/// Mocks: IInterviewRepository, ILlmClient.
/// </summary>
public class GenerateFeedbackHandlerTests
{
    private readonly Mock<IInterviewRepository> _interviewRepository = new();
    private readonly Mock<ILlmClient> _llmClient = new();
    private readonly GenerateFeedbackHandler _sut;

    public GenerateFeedbackHandlerTests()
    {
        _sut = new GenerateFeedbackHandler(
            _interviewRepository.Object,
            _llmClient.Object);
    }

    [Fact]
    public async Task Handle_WithValidInterview_ReturnsFeedbackReport()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var command = new GenerateFeedbackCommand(interviewId);
        var interview = CreateInterviewWithMessages(3);

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        var llmResponse = """
            {
                "overallScore": 82,
                "categoryScores": [
                    { "category": "Technical Knowledge", "score": 85 },
                    { "category": "Problem Solving", "score": 80 },
                    { "category": "Communication", "score": 78 },
                    { "category": "Experience Relevance", "score": 84 }
                ],
                "strengths": "Strong understanding of design patterns",
                "weaknesses": "Could improve database knowledge",
                "suggestions": "Practice SQL and NoSQL databases"
            }
            """;

        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(llmResponse);

        _interviewRepository.Setup(x => x.UpdateAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.OverallScore.Should().Be(82);
        result.Value.CategoryScores.Should().HaveCount(4);
        result.Value.Strengths.Should().Contain("design patterns");
        result.Value.Weaknesses.Should().Contain("database");
        result.Value.Suggestions.Should().Contain("SQL");

        _interviewRepository.Verify(x => x.UpdateAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInterviewNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new GenerateFeedbackCommand(Guid.NewGuid());

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
    public async Task Handle_WhenInterviewNotInProgress_ReturnsConflictError()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var command = new GenerateFeedbackCommand(interviewId);

        // Interview is NotStarted (not InProgress)
        var interview = Interview.Create(Guid.NewGuid());

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Interview.NotInProgress");

        _llmClient.Verify(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenLlmReturnsInvalidJson_ReturnsParseFailedError()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var command = new GenerateFeedbackCommand(interviewId);
        var interview = CreateInterviewWithMessages(3);

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("This is definitely not JSON");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Feedback.ParseFailed");
    }

    [Fact]
    public async Task Handle_SystemPromptContainsFeedbackRules()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var command = new GenerateFeedbackCommand(interviewId);
        var interview = CreateInterviewWithMessages(3);

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

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
        capturedMessages[0].Content.Should().Contain("Respond with ONLY a valid JSON");
    }

    [Fact]
    public async Task Handle_WhenLlmReturnsMissingCategoryScores_FillsDefaults()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var command = new GenerateFeedbackCommand(interviewId);
        var interview = CreateInterviewWithMessages(3);

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        var llmResponse = """
            {
                "overallScore": 70,
                "categoryScores": [],
                "strengths": "Good effort",
                "weaknesses": "Needs improvement",
                "suggestions": "Keep practicing"
            }
            """;

        _llmClient.Setup(x => x.ChatAsync(It.IsAny<IReadOnlyList<LlmMessage>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(llmResponse);

        _interviewRepository.Setup(x => x.UpdateAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CategoryScores.Should().HaveCount(4);
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static Interview CreateInterviewWithMessages(int candidateMessageCount)
    {
        var interview = Interview.Create(Guid.NewGuid());
        interview.Start("Q1?");

        for (int i = 0; i < candidateMessageCount; i++)
        {
            interview.AddCandidateMessage($"Answer {i + 1}");
            if (i < candidateMessageCount - 1)
                interview.AddInterviewerMessage($"Q{i + 2}?");
        }

        return interview;
    }
}
