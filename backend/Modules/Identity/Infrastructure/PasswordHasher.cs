using System;
using System.Security.Cryptography;
using Conduit.Identity.Domain.Services;

namespace Conduit.Identity.Infrastructure;

public sealed class UserPasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;        // 128 bit
    private const int HashSize = 32;        // 256 bit
    private const int Iterations = 210_000; // OWASP-Empfehlung (Stand 2024) für PBKDF2-SHA256
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword))
        {
            throw new ArgumentException("Password must not be empty.", nameof(plainPassword));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(plainPassword, salt, Iterations, Algorithm, HashSize);

        // Format: {iterations}.{salt}.{hash}  – base64-kodiert, damit als string speicherbar
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string plainPassword, string hash)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        var parts = hash.Split('.', 3);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        byte[] salt, expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expectedHash = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(plainPassword, salt, iterations, Algorithm, expectedHash.Length);

        // Konstante Laufzeit gegen Timing-Angriffe – niemals hash1 == hash2 per ==
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
