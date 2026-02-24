namespace MockInterview.Domain.Common;

/// <summary>
/// Guard clauses — defensive checks that throw DomainException when business rules are violated.
/// Used inside entities to protect invariants (e.g., "text cannot be empty", "score must be 0-100").
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws if the string is null, empty, or whitespace.
    /// </summary>
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"'{paramName}' cannot be null or empty.");
        }

        return value;
    }

    /// <summary>
    /// Throws if value is outside the [min, max] range.
    /// </summary>
    public static int InRange(int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
        {
            throw new DomainException(
                $"'{paramName}' must be between {min} and {max}. Got: {value}.");
        }

        return value;
    }

    /// <summary>
    /// Throws if the value is null.
    /// </summary>
    public static T AgainstNull<T>(T? value, string paramName)
        where T : class
    {
        if (value is null)
        {
            throw new DomainException($"'{paramName}' cannot be null.");
        }

        return value;
    }

    /// <summary>
    /// Throws if the Guid is empty (all zeros).
    /// </summary>
    public static Guid AgainstEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"'{paramName}' cannot be empty.");
        }

        return value;
    }

    /// <summary>
    /// Throws if the collection is null or has no elements.
    /// </summary>
    public static IReadOnlyList<T> AgainstEmptyCollection<T>(IReadOnlyList<T>? value, string paramName)
    {
        if (value is null || value.Count == 0)
        {
            throw new DomainException($"'{paramName}' cannot be null or empty.");
        }

        return value;
    }
}
