using FluentValidation;
using MediatR;

namespace MockInterview.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically runs FluentValidation validators
/// BEFORE the handler executes. If validation fails, it short-circuits and returns
/// a Result with validation errors — the handler never runs.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : struct
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        // Convert FluentValidation errors to our Result errors
        var errors = failures
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .ToList();

        // Use reflection to call Result<T>.Fail(errors) since TResponse is Result<Something>
        var resultType = typeof(TResponse);
        var failMethod = resultType.GetMethod(nameof(Result<object>.Fail),
            new[] { typeof(IReadOnlyList<Error>) });

        if (failMethod is null)
            throw new InvalidOperationException(
                $"TResponse '{resultType.Name}' does not have a Fail method. " +
                "Make sure all handlers return Result<T>.");

        return (TResponse)failMethod.Invoke(null, new object[] { errors })!;
    }
}
