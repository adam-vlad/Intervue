namespace MockInterview.Application.Common;

/// <summary>
/// The Result pattern — every handler returns Result&lt;T&gt; instead of throwing exceptions.
/// Either it succeeded (with a Value) or it failed (with a list of Errors).
/// This replaces try-catch in controllers and makes error handling explicit.
/// </summary>
public readonly struct Result<T>
{
    public T? Value { get; }
    public IReadOnlyList<Error> Errors { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public SuccessKind SuccessKind { get; }

    // Private constructor — use the factory methods below
    private Result(T value, SuccessKind successKind)
    {
        Value = value;
        Errors = Array.Empty<Error>();
        IsSuccess = true;
        SuccessKind = successKind;
    }

    private Result(IReadOnlyList<Error> errors)
    {
        Value = default;
        Errors = errors;
        IsSuccess = false;
        SuccessKind = default;
    }

    // ── Success factories ───────────────────────────────────────────

    /// <summary>Standard success with data → 200 OK</summary>
    public static Result<T> Ok(T value) => new(value, SuccessKind.Ok);

    /// <summary>Resource created → 201 Created</summary>
    public static Result<T> Created(T value) => new(value, SuccessKind.Created);

    /// <summary>Success with no content → 204</summary>
    public static Result<T> NoContent() => new(default!, SuccessKind.NoContent);

    // ── Failure factories ───────────────────────────────────────────

    /// <summary>Failure with a single error.</summary>
    public static Result<T> Fail(Error error) => new(new[] { error });

    /// <summary>Failure with multiple errors (e.g., validation).</summary>
    public static Result<T> Fail(IReadOnlyList<Error> errors) => new(errors);
}
