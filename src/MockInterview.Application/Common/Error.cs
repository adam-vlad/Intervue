namespace MockInterview.Application.Common;

/// <summary>
/// Represents a single error with a code and a human-readable message.
/// Immutable record — once created, it cannot be changed.
/// </summary>
public sealed record Error(string Code, string Message, ErrorKind Kind)
{
    // Convenient factory methods for creating common error types

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorKind.Validation);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorKind.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorKind.Conflict);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorKind.Failure);
}
