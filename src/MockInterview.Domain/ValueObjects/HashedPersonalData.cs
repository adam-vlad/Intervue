namespace MockInterview.Domain.ValueObjects;

/// <summary>
/// Value Object that holds a SHA-256 hash of personal data from the CV (name, email, phone).
/// We hash personal data for privacy — we never store it in plain text.
/// Value Objects are immutable and compared by value, not by reference.
/// </summary>
public sealed record HashedPersonalData
{
    /// <summary>The SHA-256 hash string of the combined personal data.</summary>
    public string Hash { get; }

    public HashedPersonalData(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty.", nameof(hash));

        Hash = hash;
    }
}
