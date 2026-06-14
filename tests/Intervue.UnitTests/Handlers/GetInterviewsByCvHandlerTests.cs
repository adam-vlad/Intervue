using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Intervue.Application.Common;
using Intervue.Application.Features.Interview.GetInterviewsByCv;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for GetInterviewsByCvHandler.
/// Mocks: ICvProfileRepository, IInterviewRepository.
/// </summary>
public class GetInterviewsByCvHandlerTests
{
    private readonly Mock<ICvProfileRepository> _cvProfileRepository = new();
    private readonly Mock<IInterviewRepository> _interviewRepository = new();
    private readonly GetInterviewsByCvHandler _sut;

    public GetInterviewsByCvHandlerTests()
    {
        _sut = new GetInterviewsByCvHandler(
            _cvProfileRepository.Object,
            _interviewRepository.Object,
            NullLoggerFactory.Instance.CreateLogger<GetInterviewsByCvHandler>());
    }

    [Fact]
    public async Task Handle_WhenCvProfileNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _cvProfileRepository.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CvProfile?)null);

        var query = new GetInterviewsByCvQuery(nonExistentId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Cv.NotFound");
        result.Errors[0].Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCvExistsWithInterviews_ReturnsSummaryDtos()
    {
        // Arrange
        var cvProfile = CvProfile.Create("CV text", new HashedPersonalData("hash"));
        _cvProfileRepository.Setup(r => r.GetByIdAsync(cvProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        var interview1 = Interview.Create(cvProfile.Id);
        interview1.Start("Q1?");
        var interview2 = Interview.Create(cvProfile.Id);
        interview2.Start("Q2?");

        _interviewRepository.Setup(r => r.GetByCvProfileIdAsync(cvProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Interview> { interview1, interview2 });

        var query = new GetInterviewsByCvQuery(cvProfile.Id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].CvProfileId.Should().Be(cvProfile.Id);
        result.Value[1].CvProfileId.Should().Be(cvProfile.Id);
    }

    [Fact]
    public async Task Handle_WhenCvExistsWithNoInterviews_ReturnsEmptyList()
    {
        // Arrange
        var cvProfile = CvProfile.Create("CV text", new HashedPersonalData("hash"));
        _cvProfileRepository.Setup(r => r.GetByIdAsync(cvProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        _interviewRepository.Setup(r => r.GetByCvProfileIdAsync(cvProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Interview>());

        var query = new GetInterviewsByCvQuery(cvProfile.Id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DoesNotCallInterviewRepo_WhenCvNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _cvProfileRepository.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CvProfile?)null);

        var query = new GetInterviewsByCvQuery(nonExistentId);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _interviewRepository.Verify(r => r.GetByCvProfileIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
