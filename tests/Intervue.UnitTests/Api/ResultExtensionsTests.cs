using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Intervue.Application.Common;
using Intervue.Api.Extensions;

namespace Intervue.UnitTests.Api;

/// <summary>
/// Unit tests for ResultExtensions.ToActionResult().
/// Tests that Result values are mapped to the correct HTTP status codes.
/// </summary>
public class ResultExtensionsTests
{
    // ── Success mappings ────────────────────────────────────────────

    [Fact]
    public void ToActionResult_WhenOk_Returns200WithValue()
    {
        // Arrange
        var result = Result<string>.Ok("hello");

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be("hello");
    }

    [Fact]
    public void ToActionResult_WhenCreated_Returns201WithValue()
    {
        // Arrange
        var result = Result<Guid>.Created(Guid.NewGuid());

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public void ToActionResult_WhenNoContent_Returns204()
    {
        // Arrange
        var result = Result<string>.NoContent();

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
    }

    // ── Error mappings ──────────────────────────────────────────────

    [Fact]
    public void ToActionResult_WhenValidationError_Returns400()
    {
        // Arrange
        var result = Result<string>.Fail(Error.Validation("Field.Required", "Field is required."));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Status.Should().Be(400);
        problem.Title.Should().Be("Validation");
    }

    [Fact]
    public void ToActionResult_WhenNotFoundError_Returns404()
    {
        // Arrange
        var result = Result<string>.Fail(Error.NotFound("Entity.NotFound", "Not found."));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public void ToActionResult_WhenConflictError_Returns409()
    {
        // Arrange
        var result = Result<string>.Fail(Error.Conflict("Duplicate", "Already exists."));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public void ToActionResult_WhenFailureError_Returns500()
    {
        // Arrange
        var result = Result<string>.Fail(Error.Failure("Unexpected", "Something went wrong."));

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    public void ToActionResult_WhenMultipleErrors_IncludesAllInProblemDetails()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.Validation("Field1", "Field1 is required."),
            Error.Validation("Field2", "Field2 is too short.")
        };
        var result = Result<string>.Fail(errors.AsReadOnly());

        // Act
        var actionResult = result.ToActionResult();

        // Assert
        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Contain("Field1 is required.");
        problem.Detail.Should().Contain("Field2 is too short.");
    }
}
