using System.Security.Cryptography;

namespace DineFlow.Services.Tables;

public static class TableOtpGenerator
{
    public const int Length = 6;
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string Generate()
    {
        Span<char> otp = stackalloc char[Length];
        Span<byte> buffer = stackalloc byte[Length];
        RandomNumberGenerator.Fill(buffer);

        for (int index = 0; index < Length; index++)
        {
            otp[index] = Alphabet[buffer[index] % Alphabet.Length];
        }

        return new string(otp);
    }
}
