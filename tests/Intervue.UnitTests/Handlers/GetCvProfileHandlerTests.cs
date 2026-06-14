using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Intervue.Application.Common;
using Intervue.Application.Features.Cv.GetCvProfile;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for GetCvProfileHandler.
/// Mocks: ICvProfileRepository.
/// </summary>
public class GetCvProfileHandlerTests
{
    private readonly Mock<ICvProfileRepository> _cvProfileRepository = new();
    private readonly GetCvProfileHandler _sut;

    public GetCvProfileHandlerTests()
    {
        _sut = new GetCvProfileHandler(
            _cvProfileRepository.Object,
            NullLoggerFactory.Instance.CreateLogger<GetCvProfileHandler>());
    }

    [Fact]
    public async Task Handle_WhenCvProfileExists_ReturnsOkWithDto()
    {
        // Arrange
        var cvProfile = CvProfile.Create("Some CV text", new HashedPersonalData("hash123"));
        var techs = new List<Technology> { Technology.Create("C#", 5) };
        cvProfile.SetParsedData(DifficultyLevel.Mid, "B.Sc. CS", techs, new(), new());

        _cvProfileRepository.Setup(r => r.GetByIdAsync(cvProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        var query = new GetCvProfileQuery(cvProfile.Id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(cvProfile.Id);
        result.Value.DifficultyLevel.Should().Be(DifficultyLevel.Mid);
        result.Value.Technologies.Should().HaveCount(1);
        result.Value.Technologies[0].Name.Should().Be("C#");
    }

    [Fact]
    public async Task Handle_WhenCvProfileNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _cvProfileRepository.Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CvProfile?)null);

        var query = new GetCvProfileQuery(nonExistentId);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Cv.NotFound");
        result.Errors[0].Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCvProfileExists_MapsDtoCorrectly()
    {
        // Arrange
        var cvProfile = CvProfile.Create("Raw CV content", new HashedPersonalData("hash456"));
        _cvProfileRepository.Setup(r => r.GetByIdAsync(cvProfile.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cvProfile);

        var query = new GetCvProfileQuery(cvProfile.Id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.RawText.Should().Be("Raw CV content");
        result.Value.DifficultyLevel.Should().Be(DifficultyLevel.Junior); // default before parsing
        result.Value.Education.Should().BeNull();
        result.Value.Technologies.Should().BeEmpty();
        result.Value.Experiences.Should().BeEmpty();
        result.Value.Projects.Should().BeEmpty();
    }
}
