using FluentAssertions;
using Moq;
using Intervue.Application.Common;
using Intervue.Application.Features.DTOs;
using Intervue.Application.Features.Interview.GetInterview;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for GetInterviewHandler.
/// Mocks: IInterviewRepository.
/// </summary>
public class GetInterviewHandlerTests
{
    private readonly Mock<IInterviewRepository> _interviewRepository = new();
    private readonly GetInterviewHandler _sut;

    public GetInterviewHandlerTests()
    {
        _sut = new GetInterviewHandler(_interviewRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenInterviewExists_ReturnsOkWithDto()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var query = new GetInterviewQuery(interviewId);
        var interview = Interview.Create(Guid.NewGuid());
        interview.Start("First question?");

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.SuccessKind.Should().Be(SuccessKind.Ok);
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(InterviewStatus.InProgress);
        result.Value.Messages.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenInterviewNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetInterviewQuery(Guid.NewGuid());

        _interviewRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Interview?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "Interview.NotFound");
    }

    [Fact]
    public async Task Handle_ReturnsDtoWithCorrectMapping()
    {
        // Arrange
        var interviewId = Guid.NewGuid();
        var cvProfileId = Guid.NewGuid();
        var query = new GetInterviewQuery(interviewId);

        var interview = Interview.Create(cvProfileId);
        interview.SetPromptProfile("Junior_v1");
        interview.Start("Q1");
        interview.AddCandidateMessage("A1");
        interview.AddInterviewerMessage("Q2");

        _interviewRepository.Setup(x => x.GetByIdAsync(interviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CvProfileId.Should().Be(cvProfileId);
        result.Value.PromptProfile.Should().Be("Junior_v1");
        result.Value.Messages.Should().HaveCount(3);
    }
}
