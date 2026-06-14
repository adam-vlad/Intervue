using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Intervue.Application.Common;
using Intervue.Application.Common.Behaviors;
using Intervue.Domain.Common;

namespace Intervue.UnitTests.Behaviors;

/// <summary>
/// Unit tests for ExceptionToResultBehavior.
/// Tests that different exception types are mapped to the correct error kinds.
/// </summary>
public class ExceptionToResultBehaviorTests
{
    private readonly ExceptionToResultBehavior<TestRequest, Result<string>> _sut;

    public ExceptionToResultBehaviorTests()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<ExceptionToResultBehavior<TestRequest, Result<string>>>();
        _sut = new ExceptionToResultBehavior<TestRequest, Result<string>>(logger);
    }

    [Fact]
    public async Task Handle_WhenDomainException_ReturnsValidationError()
    {
        // Arrange
        var request = new TestRequest();
        RequestHandlerDelegate<Result<string>> next = _ =>
            throw new DomainException("Business rule violated");

        // Act
        var result = await _sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be("Domain.RuleViolation");
        result.Errors[0].Message.Should().Be("Business rule violated");
        result.Errors[0].Kind.Should().Be(ErrorKind.Validation);
    }

    [Fact]
    public async Task Handle_WhenKeyNotFoundException_ReturnsNotFoundError()
    {
        // Arrange
        var request = new TestRequest();
        RequestHandlerDelegate<Result<string>> next = _ =>
            throw new KeyNotFoundException("Entity not found");

        // Act
        var result = await _sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Entity.NotFound");
        result.Errors[0].Kind.Should().Be(ErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUnhandledException_ReturnsFailureWithGenericMessage()
    {
        // Arrange
        var request = new TestRequest();
        RequestHandlerDelegate<Result<string>> next = _ =>
            throw new InvalidOperationException("Internal details that should not leak");

        // Act
        var result = await _sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Unexpected.Error");
        result.Errors[0].Message.Should().Be("An unexpected error occurred.");
        result.Errors[0].Message.Should().NotContain("Internal details");
        result.Errors[0].Kind.Should().Be(ErrorKind.Failure);
    }

    [Fact]
    public async Task Handle_WhenNoException_ReturnsHandlerResult()
    {
        // Arrange
        var request = new TestRequest();
        RequestHandlerDelegate<Result<string>> next = _ =>
            Task.FromResult(Result<string>.Ok("success"));

        // Act
        var result = await _sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("success");
    }

    [Fact]
    public async Task Handle_WhenFluentValidationException_ReturnsMultipleValidationErrors()
    {
        // Arrange
        var request = new TestRequest();
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new("Field1", "Field1 is required"),
            new("Field2", "Field2 must be positive")
        };
        RequestHandlerDelegate<Result<string>> next = _ =>
            throw new FluentValidation.ValidationException(failures);

        // Act
        var result = await _sut.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Code.Should().Be("Field1");
        result.Errors[1].Code.Should().Be("Field2");
    }

    // ── Test helpers ────────────────────────────────────────────────

    public record TestRequest : IRequest<Result<string>>;
}
