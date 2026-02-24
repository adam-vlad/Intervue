namespace MockInterview.Domain.Common;

/// <summary>
/// Custom exception for domain rule violations.
/// Thrown by Guard clauses when a business rule is broken.
/// Caught by the MediatR pipeline and converted to a Result error.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
