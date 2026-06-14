using FluentAssertions;
using FluentValidation;
using Intervue.Application.Features.Cv.GetCvProfile;

namespace Intervue.UnitTests.Validators;

/// <summary>
/// Unit tests for GetCvProfileValidator.
/// </summary>
public class GetCvProfileValidatorTests
{
    private readonly GetCvProfileValidator _sut = new();

    [Fact]
    public void Validate_WithValidCvProfileId_Passes()
    {
        // Arrange
        var query = new GetCvProfileQuery(Guid.NewGuid());

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCvProfileId_Fails()
    {
        // Arrange
        var query = new GetCvProfileQuery(Guid.Empty);

        // Act
        var result = _sut.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CvProfileId");
    }
}
