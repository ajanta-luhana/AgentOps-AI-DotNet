using System.Globalization;
using System.Security.Cryptography;
using AgentOps.Application.Abstractions;

namespace AgentOps.Infrastructure.Authentication;

/// <summary>
/// PBKDF2-SHA256 with a random salt per password. Stored format: v1.{iterations}.{saltBase64}.{hashBase64}.
/// The iteration count is stored so it can be raised later without breaking existing passwords.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Version = "v1";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 210_000;
    private const int MaxAcceptedIterations = 2_000_000;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return string.Join(
            '.',
            Version,
            Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string storedHash)
    {
        ArgumentNullException.ThrowIfNull(password);
        if (string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split('.');
        if (parts.Length != 4
            || parts[0] != Version
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations)
            || iterations < 1
            || iterations > MaxAcceptedIterations)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length == 0 || expected.Length == 0)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
