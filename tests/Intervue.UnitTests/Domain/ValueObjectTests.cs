using FluentAssertions;
using Intervue.Domain.Common;
using Intervue.Domain.ValueObjects;

namespace Intervue.UnitTests.Domain;

/// <summary>
/// Unit tests for Value Objects: HashedPersonalData, InterviewScore.
/// Tests immutability, construction, validation, and value equality.
/// </summary>
public class ValueObjectTests
{
    // ── HashedPersonalData ──────────────────────────────────────────

    [Fact]
    public void HashedPersonalData_WithValidHash_CreatesSuccessfully()
    {
        // Act
        var hashed = new HashedPersonalData("sha256hashvalue");

        // Assert
        hashed.Hash.Should().Be("sha256hashvalue");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashedPersonalData_WithInvalidHash_ThrowsDomainException(string? hash)
    {
        // Act
        var act = () => new HashedPersonalData(hash!);

        // Assert — after the fix, this should throw DomainException (not ArgumentException)
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void HashedPersonalData_WithSameHash_AreEqual()
    {
        // Arrange
        var hash1 = new HashedPersonalData("abc123");
        var hash2 = new HashedPersonalData("abc123");

        // Assert — records compare by value
        hash1.Should().Be(hash2);
        (hash1 == hash2).Should().BeTrue();
    }

    [Fact]
    public void HashedPersonalData_WithDifferentHash_AreNotEqual()
    {
        // Arrange
        var hash1 = new HashedPersonalData("abc123");
        var hash2 = new HashedPersonalData("xyz789");

        // Assert
        hash1.Should().NotBe(hash2);
    }

    // ── InterviewScore ──────────────────────────────────────────────

    [Fact]
    public void InterviewScore_WithValidInputs_CreatesSuccessfully()
    {
        // Act
        var score = new InterviewScore("Technical Knowledge", 85);

        // Assert
        score.Category.Should().Be("Technical Knowledge");
        score.Score.Should().Be(85);
    }

    [Fact]
    public void InterviewScore_WithBoundaryScores_CreatesSuccessfully()
    {
        // Act & Assert — 0 and 100 are valid boundaries
        var min = new InterviewScore("Min", 0);
        var max = new InterviewScore("Max", 100);

        min.Score.Should().Be(0);
        max.Score.Should().Be(100);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(-50)]
    [InlineData(200)]
    public void InterviewScore_WithOutOfRangeScore_ThrowsDomainException(int score)
    {
        // Act
        var act = () => new InterviewScore("Category", score);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InterviewScore_WithInvalidCategory_ThrowsDomainException(string? category)
    {
        // Act
        var act = () => new InterviewScore(category!, 50);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void InterviewScore_WithSameValues_AreEqual()
    {
        // Arrange
        var score1 = new InterviewScore("Communication", 70);
        var score2 = new InterviewScore("Communication", 70);

        // Assert — records compare by value
        score1.Should().Be(score2);
    }

    [Fact]
    public void InterviewScore_WithDifferentValues_AreNotEqual()
    {
        // Arrange
        var score1 = new InterviewScore("Communication", 70);
        var score2 = new InterviewScore("Communication", 80);

        // Assert
        score1.Should().NotBe(score2);
    }
}
