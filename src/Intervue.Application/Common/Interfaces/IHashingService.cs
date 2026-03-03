namespace Intervue.Application.Common.Interfaces;

/// <summary>
/// Contract for hashing sensitive personal data (name, email, phone) from CVs.
/// Infrastructure implements this with SHA-256.
/// </summary>
public interface IHashingService
{
    /// <summary>
    /// Takes a plain text string and returns its SHA-256 hash.
    /// </summary>
    string Hash(string input);
}
