using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DineFlow.BusinessObjects.Auth;

namespace DineFlow.Api.Services;

public sealed record StaffAuthPrincipal(
    int UserId,
    string Username,
    string FullName,
    string Role,
    DateTime ExpiresAt);

public interface IStaffAuthTokenService
{
    string CreateToken(CurrentUser user);
    StaffAuthPrincipal? ValidateToken(string? token);
}

public sealed class StaffAuthTokenService : IStaffAuthTokenService
{
    private readonly byte[] _signingKey;
    private readonly TimeSpan _lifetime;

    public StaffAuthTokenService(IConfiguration configuration)
    {
        string key = configuration["StaffAuth:SigningKey"]
            ?? Environment.GetEnvironmentVariable("STAFF_AUTH_SIGNING_KEY")
            ?? "DineFlow.StaffAuth.Development.SigningKey.ChangeMe";
        _signingKey = Encoding.UTF8.GetBytes(key);
        _lifetime = TimeSpan.FromHours(
            double.TryParse(configuration["StaffAuth:LifetimeHours"], out double hours) ? hours : 12);
    }

    public string CreateToken(CurrentUser user)
    {
        StaffAuthPrincipal payload = new(
            user.UserId,
            user.Username,
            user.FullName,
            AuthRoles.Normalize(user.Role),
            DateTime.UtcNow.Add(_lifetime));

        string payloadJson = JsonSerializer.Serialize(payload);
        string payloadPart = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        string signaturePart = Sign(payloadPart);
        return $"{payloadPart}.{signaturePart}";
    }

    public StaffAuthPrincipal? ValidateToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        string[] parts = token.Split('.', 2);
        if (parts.Length != 2)
        {
            return null;
        }

        string expectedSignature = Sign(parts[0]);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(parts[1])))
        {
            return null;
        }

        try
        {
            StaffAuthPrincipal? principal = JsonSerializer.Deserialize<StaffAuthPrincipal>(
                Encoding.UTF8.GetString(Base64UrlDecode(parts[0])));
            return principal is not null && principal.ExpiresAt > DateTime.UtcNow
                ? principal
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private string Sign(string payloadPart)
    {
        using HMACSHA256 hmac = new(_signingKey);
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadPart)));
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        string padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - padded.Length % 4) % 4), '=');
        return Convert.FromBase64String(padded);
    }
}
