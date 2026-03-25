using FluentAssertions;
using Intervue.Domain.Entities;
using Intervue.Domain.Common;
using Intervue.Domain.Enums;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Domain;

/// <summary>
/// Unit tests for CvProfile aggregate root.
/// Tests: Create, SetParsedData, GetTopTechnologies, guard clauses.
/// </summary>
public class CvProfileTests
{
    // ── Create ──────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidInputs_ReturnsCvProfile()
    {
        // Arrange
        var rawText = "John Doe, Software Developer, 3 years experience in C#";
        var hashedData = new HashedPersonalData("abc123hash");

        // Act
        var cvProfile = CvProfile.Create(rawText, hashedData);

        // Assert
        cvProfile.Should().NotBeNull();
        cvProfile.Id.Should().NotBe(Guid.Empty);
        cvProfile.RawText.Should().Be(rawText);
        cvProfile.HashedPersonalData.Should().Be(hashedData);
        cvProfile.DifficultyLevel.Should().Be(DifficultyLevel.Junior); // default
        cvProfile.Education.Should().BeNull();
        cvProfile.Technologies.Should().BeEmpty();
        cvProfile.Experiences.Should().BeEmpty();
        cvProfile.Projects.Should().BeEmpty();
        cvProfile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhiteSpaceRawText_ThrowsDomainException(string? rawText)
    {
        // Arrange
        var hashedData = new HashedPersonalData("abc123hash");

        // Act
        var act = () => CvProfile.Create(rawText!, hashedData);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*rawText*");
    }

    [Fact]
    public void Create_WithNullHashedData_ThrowsDomainException()
    {
        // Act
        var act = () => CvProfile.Create("Some CV text", null!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*hashedPersonalData*");
    }

    // ── SetParsedData ───────────────────────────────────────────────

    [Fact]
    public void SetParsedData_WithValidData_UpdatesAllFields()
    {
        // Arrange
        var cvProfile = CvProfile.Create("Some CV text", new HashedPersonalData("hash123"));
        var technologies = new List<Technology>
        {
            Technology.Create("C#", 5),
            Technology.Create("React", 2)
        };
        var experiences = new List<Experience>
        {
            Experience.Create("Developer", "Acme Inc", 24, "Built APIs")
        };
        var projects = new List<Project>
        {
            Project.Create("MyApp", "A web app", new List<string> { "C#", "React" })
        };

        // Act
        cvProfile.SetParsedData(DifficultyLevel.Mid, "B.Sc. Computer Science", technologies, experiences, projects);

        // Assert
        cvProfile.DifficultyLevel.Should().Be(DifficultyLevel.Mid);
        cvProfile.Education.Should().Be("B.Sc. Computer Science");
        cvProfile.Technologies.Should().HaveCount(2);
        cvProfile.Technologies[0].Name.Should().Be("C#");
        cvProfile.Technologies[1].Name.Should().Be("React");
        cvProfile.Experiences.Should().HaveCount(1);
        cvProfile.Experiences[0].Company.Should().Be("Acme Inc");
        cvProfile.Projects.Should().HaveCount(1);
        cvProfile.Projects[0].Name.Should().Be("MyApp");
    }

    [Fact]
    public void SetParsedData_CalledTwice_ReplacesOldData()
    {
        // Arrange
        var cvProfile = CvProfile.Create("Some CV text", new HashedPersonalData("hash123"));
        var firstTechs = new List<Technology> { Technology.Create("Java", 3) };
        var secondTechs = new List<Technology> { Technology.Create("Go", 1), Technology.Create("Rust", 2) };

        cvProfile.SetParsedData(DifficultyLevel.Junior, null, firstTechs, new(), new());

        // Act
        cvProfile.SetParsedData(DifficultyLevel.Senior, "PhD", secondTechs, new(), new());

        // Assert
        cvProfile.DifficultyLevel.Should().Be(DifficultyLevel.Senior);
        cvProfile.Education.Should().Be("PhD");
        cvProfile.Technologies.Should().HaveCount(2);
        cvProfile.Technologies[0].Name.Should().Be("Go");
    }

    [Fact]
    public void SetParsedData_WithNullEducation_AllowsNull()
    {
        // Arrange
        var cvProfile = CvProfile.Create("Some CV text", new HashedPersonalData("hash123"));

        // Act
        cvProfile.SetParsedData(DifficultyLevel.Junior, null, new(), new(), new());

        // Assert
        cvProfile.Education.Should().BeNull();
    }

    [Fact]
    public void SetParsedData_WithEmptyCollections_ClearsExistingData()
    {
        // Arrange
        var cvProfile = CvProfile.Create("Some CV text", new HashedPersonalData("hash123"));
        var techs = new List<Technology> { Technology.Create("Python", 2) };
        cvProfile.SetParsedData(DifficultyLevel.Mid, null, techs, new(), new());

        // Act
        cvProfile.SetParsedData(DifficultyLevel.Junior, null, new(), new(), new());

        // Assert
        cvProfile.Technologies.Should().BeEmpty();
    }

    // ── GetTopTechnologies ──────────────────────────────────────────

    [Fact]
    public void GetTopTechnologies_ReturnsTopNByYearsOfExperience()
    {
        // Arrange
        var cvProfile = CvProfile.Create("CV", new HashedPersonalData("hash"));
        var techs = new List<Technology>
        {
            Technology.Create("C#", 5),
            Technology.Create("Python", 2),
            Technology.Create("Go", 8),
            Technology.Create("JS", 1)
        };
        cvProfile.SetParsedData(DifficultyLevel.Senior, null, techs, new(), new());

        // Act
        var top2 = cvProfile.GetTopTechnologies(2);

        // Assert
        top2.Should().HaveCount(2);
        top2[0].Name.Should().Be("Go");       // 8 years
        top2[1].Name.Should().Be("C#");        // 5 years
    }

    [Fact]
    public void GetTopTechnologies_WhenCountExceedsList_ReturnsAll()
    {
        // Arrange
        var cvProfile = CvProfile.Create("CV", new HashedPersonalData("hash"));
        var techs = new List<Technology> { Technology.Create("C#", 3) };
        cvProfile.SetParsedData(DifficultyLevel.Junior, null, techs, new(), new());

        // Act
        var result = cvProfile.GetTopTechnologies(10);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public void GetTopTechnologies_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        var cvProfile = CvProfile.Create("CV", new HashedPersonalData("hash"));

        // Act
        var result = cvProfile.GetTopTechnologies(5);

        // Assert
        result.Should().BeEmpty();
    }

    // ── Entity equality ─────────────────────────────────────────────

    [Fact]
    public void TwoCvProfiles_WithDifferentIds_AreNotEqual()
    {
        // Arrange
        var profile1 = CvProfile.Create("Text 1", new HashedPersonalData("hash1"));
        var profile2 = CvProfile.Create("Text 2", new HashedPersonalData("hash2"));

        // Assert
        profile1.Should().NotBe(profile2);
    }
}
