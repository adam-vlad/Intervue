using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Intervue.Application.Common.Constants;
using Intervue.Domain.Common;

namespace Intervue.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that catches known exceptions and converts them
/// into Result failures — so controllers never see raw exceptions.
/// DomainException → Validation error, KeyNotFoundException → NotFound error, etc.
/// </summary>
public class ExceptionToResultBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : struct
{
    private readonly ILogger<ExceptionToResultBehavior<TRequest, TResponse>> _logger;

    public ExceptionToResultBehavior(ILogger<ExceptionToResultBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (DomainException ex)
        {
            return CreateFailResult(Error.Validation(ErrorCodes.DomainRuleViolation, ex.Message));
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
                .ToList();

            return CreateFailResult(errors);
        }
        catch (KeyNotFoundException ex)
        {
            return CreateFailResult(Error.NotFound(ErrorCodes.EntityNotFound, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in handler {HandlerType}.", typeof(TRequest).Name);
            return CreateFailResult(Error.Failure(ErrorCodes.UnexpectedError, "An unexpected error occurred."));
        }
    }

    private static TResponse CreateFailResult(Error error)
    {
        return CreateFailResult(new List<Error> { error });
    }

    private static TResponse CreateFailResult(List<Error> errors)
    {
        var resultType = typeof(TResponse);
        var failMethod = resultType.GetMethod(nameof(Result<object>.Fail),
            new[] { typeof(IReadOnlyList<Error>) });

        if (failMethod is null)
            throw new InvalidOperationException(
                $"TResponse '{resultType.Name}' does not have a Fail method.");

        return (TResponse)failMethod.Invoke(null, new object[] { errors })!;
    }
}
