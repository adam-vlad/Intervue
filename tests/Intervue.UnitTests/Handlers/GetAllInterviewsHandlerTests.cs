using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Intervue.Application.Common;
using Intervue.Application.Features.Interview.GetAllInterviews;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for GetAllInterviewsHandler.
/// Mocks: IInterviewRepository.
/// </summary>
public class GetAllInterviewsHandlerTests
{
    private readonly Mock<IInterviewRepository> _interviewRepository = new();
    private readonly GetAllInterviewsHandler _sut;

    public GetAllInterviewsHandlerTests()
    {
        _sut = new GetAllInterviewsHandler(
            _interviewRepository.Object,
            NullLoggerFactory.Instance.CreateLogger<GetAllInterviewsHandler>());
    }

    [Fact]
    public async Task Handle_WhenInterviewsExist_ReturnsOkWithSummaryDtos()
    {
        // Arrange
        var interview1 = Interview.Create(Guid.NewGuid());
        interview1.Start("Q1?");
        var interview2 = Interview.Create(Guid.NewGuid());
        interview2.Start("Q2?");

        _interviewRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Interview> { interview1, interview2 });

        var query = new GetAllInterviewsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Status.Should().Be(InterviewStatus.InProgress);
        result.Value[0].MessageCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenNoInterviews_ReturnsOkWithEmptyList()
    {
        // Arrange
        _interviewRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Interview>());

        var query = new GetAllInterviewsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsSummaryWithCorrectFeedbackInfo()
    {
        // Arrange
        var interview = Interview.Create(Guid.NewGuid());
        interview.Start("Q1?");
        interview.AddCandidateMessage("A1");
        interview.AddInterviewerMessage("Q2?");
        interview.AddCandidateMessage("A2");
        interview.AddInterviewerMessage("Q3?");
        interview.AddCandidateMessage("A3");

        var scores = new List<InterviewScore>
        {
            new("Technical", 80),
            new("Communication", 75)
        };
        var feedback = FeedbackReport.Create(85, scores, "Good", "Needs work", "Practice more");
        interview.Complete(feedback);

        _interviewRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Interview> { interview });

        var query = new GetAllInterviewsQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value![0];
        dto.Status.Should().Be(InterviewStatus.Completed);
        dto.HasFeedback.Should().BeTrue();
        dto.OverallScore.Should().Be(85);
        dto.MessageCount.Should().Be(6);
        dto.CompletedAt.Should().NotBeNull();
    }
}
