using System.Security.Cryptography;
using System.Text;

namespace BWell.Loyalty.Infrastructure.Security;

public static class PasswordHasher
{
    private const int Iterations = 150_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(stored))
            return false;

        // Legacy format from earlier seed: salt.hash
        var parts = stored.Split('.');
        if (parts.Length == 2)
        {
            try
            {
                var salt = Convert.FromBase64String(parts[0]);
                var expected = Convert.FromBase64String(parts[1]);
                var actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
                return CryptographicOperations.FixedTimeEquals(expected, actual);
            }
            catch
            {
                return false;
            }
        }

        if (parts.Length != 4 || parts[0] != "v1")
            return false;

        if (!int.TryParse(parts[1], out var iterations))
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, KeySize);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch
        {
            return false;
        }
    }
}
