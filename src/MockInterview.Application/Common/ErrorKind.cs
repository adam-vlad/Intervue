namespace MockInterview.Application.Common;

/// <summary>
/// Defines the kind of error — maps to HTTP status codes later in the API layer.
/// </summary>
public enum ErrorKind
{
    /// <summary>Input validation failed → HTTP 400</summary>
    Validation,

    /// <summary>Entity not found → HTTP 404</summary>
    NotFound,

    /// <summary>Business rule conflict → HTTP 409</summary>
    Conflict,

    /// <summary>Unexpected internal error → HTTP 500</summary>
    Failure
}
