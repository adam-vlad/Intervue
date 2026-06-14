using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Intervue.Application.Common;
using Intervue.Application.Common.Behaviors;

namespace Intervue.UnitTests.Behaviors;

/// <summary>
/// Unit tests for ValidationBehavior.
/// Tests that FluentValidation errors short-circuit the pipeline
/// and that valid requests pass through to the handler.
/// </summary>
public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WhenNoValidators_CallsNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestCommand>>();
        var sut = new ValidationBehavior<TestCommand, Result<string>>(validators);
        var wasCalled = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            wasCalled = true;
            return Task.FromResult(Result<string>.Ok("done"));
        };

        // Act
        var result = await sut.Handle(new TestCommand("valid"), next, CancellationToken.None);

        // Assert
        wasCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_CallsNext()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var sut = new ValidationBehavior<TestCommand, Result<string>>(new[] { validator });
        var wasCalled = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            wasCalled = true;
            return Task.FromResult(Result<string>.Ok("done"));
        };

        // Act
        var result = await sut.Handle(new TestCommand("valid-content"), next, CancellationToken.None);

        // Assert
        wasCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShortCircuitsAndReturnsErrors()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var sut = new ValidationBehavior<TestCommand, Result<string>>(new[] { validator });
        var wasCalled = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            wasCalled = true;
            return Task.FromResult(Result<string>.Ok("done"));
        };

        // Act — empty content should fail validation
        var result = await sut.Handle(new TestCommand(""), next, CancellationToken.None);

        // Assert
        wasCalled.Should().BeFalse("handler should not be called when validation fails");
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be("Content");
        result.Errors[0].Kind.Should().Be(ErrorKind.Validation);
    }

    [Fact]
    public async Task Handle_WhenMultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange — use a validator that checks two things
        var validator = new StrictTestCommandValidator();
        var sut = new ValidationBehavior<TestCommand, Result<string>>(new[] { validator });

        RequestHandlerDelegate<Result<string>> next = _ =>
            Task.FromResult(Result<string>.Ok("done"));

        // Act — empty content triggers both rules
        var result = await sut.Handle(new TestCommand(""), next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    // ── Test helpers ────────────────────────────────────────────────

    public record TestCommand(string Content) : IRequest<Result<string>>;

    private class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.");
        }
    }

    private class StrictTestCommandValidator : AbstractValidator<TestCommand>
    {
        public StrictTestCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.");

            RuleFor(x => x.Content)
                .MinimumLength(5).WithMessage("Content must be at least 5 characters.");
        }
    }
}
