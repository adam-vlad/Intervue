using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Intervue.Application.Common;
using Intervue.Application.Features.Cv.GetAllCvProfiles;
using Intervue.Application.Features.DTOs;
using Intervue.Domain.Entities;
using Intervue.Domain.Enums;
using Intervue.Domain.Repositories;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Handlers;

/// <summary>
/// Unit tests for GetAllCvProfilesHandler.
/// Mocks: ICvProfileRepository.
/// </summary>
public class GetAllCvProfilesHandlerTests
{
    private readonly Mock<ICvProfileRepository> _cvProfileRepository = new();
    private readonly GetAllCvProfilesHandler _sut;

    public GetAllCvProfilesHandlerTests()
    {
        _sut = new GetAllCvProfilesHandler(
            _cvProfileRepository.Object,
            NullLoggerFactory.Instance.CreateLogger<GetAllCvProfilesHandler>());
    }

    [Fact]
    public async Task Handle_WhenProfilesExist_ReturnsOkWithSummaryDtos()
    {
        // Arrange
        var profile1 = CvProfile.Create("CV text 1", new HashedPersonalData("hash1"));
        var profile2 = CvProfile.Create("CV text 2", new HashedPersonalData("hash2"));
        profile1.SetParsedData(DifficultyLevel.Junior, "B.Sc.", new List<Technology> { Technology.Create("C#", 3) }, new(), new());
        profile2.SetParsedData(DifficultyLevel.Senior, "M.Sc.", new List<Technology> { Technology.Create("Python", 8) }, new(), new());

        _cvProfileRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CvProfile> { profile1, profile2 });

        var query = new GetAllCvProfilesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        var allTechnologies = result.Value!.SelectMany(p => p.Technologies).ToList();
        allTechnologies.Should().Contain("C#");
        allTechnologies.Should().Contain("Python");
    }

    [Fact]
    public async Task Handle_WhenNoProfiles_ReturnsOkWithEmptyList()
    {
        // Arrange
        _cvProfileRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CvProfile>());

        var query = new GetAllCvProfilesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsSummaryDtoWithoutRawText()
    {
        // Arrange
        var profile = CvProfile.Create("Very long raw CV text", new HashedPersonalData("hash"));
        profile.SetParsedData(DifficultyLevel.Mid, "Ph.D.", new List<Technology> { Technology.Create("Java", 5) }, new(), new());

        _cvProfileRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CvProfile> { profile });

        var query = new GetAllCvProfilesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value![0];
        dto.Id.Should().Be(profile.Id);
        dto.DifficultyLevel.Should().Be(DifficultyLevel.Mid);
        dto.Education.Should().Be("Ph.D.");
        dto.Technologies.Should().ContainSingle("Java");
    }

    [Fact]
    public async Task Handle_ReturnsProfilesOrderedByCreatedAtDescending()
    {
        // Arrange
        var older = CvProfile.Create("Older CV", new HashedPersonalData("hash1"));
        var newer = CvProfile.Create("Newer CV", new HashedPersonalData("hash2"));

        _cvProfileRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CvProfile> { older, newer });

        var query = new GetAllCvProfilesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
