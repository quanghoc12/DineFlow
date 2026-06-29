using System.Security.Cryptography;
using System.Text;

namespace DineFlow.Services.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        if (!NeedsUpgrade(passwordHash))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }

        // Compatibility for existing development seed data. A successful login
        // immediately upgrades this value to BCrypt in AuthService.
        byte[] supplied = Encoding.UTF8.GetBytes(password);
        byte[] stored = Encoding.UTF8.GetBytes(passwordHash);
        return supplied.Length == stored.Length &&
               CryptographicOperations.FixedTimeEquals(supplied, stored);
    }

    public bool NeedsUpgrade(string passwordHash)
    {
        return !passwordHash.StartsWith("$2", StringComparison.Ordinal);
    }
}
