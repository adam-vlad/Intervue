namespace Intervue.Application.Common;

/// <summary>
/// Defines the kind of success — maps to HTTP status codes.
/// </summary>
public enum SuccessKind
{
    /// <summary>Standard success → HTTP 200</summary>
    Ok,

    /// <summary>Resource was created → HTTP 201</summary>
    Created,

    /// <summary>Success with no content → HTTP 204</summary>
    NoContent
}
