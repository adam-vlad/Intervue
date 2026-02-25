using System.Security.Cryptography;
using System.Text;
using MockInterview.Application.Common.Interfaces;

namespace MockInterview.Infrastructure.Services;

/// <summary>
/// Implements IHashingService using SHA-256 algorithm.
/// Used to hash personal data (name, email, phone) from CVs for privacy.
/// </summary>
public class Sha256HashingService : IHashingService
{
    public string Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);

        // Convert to hex string (e.g., "a3f2b8c1...")
        return Convert.ToHexStringLower(hashBytes);
    }
}
