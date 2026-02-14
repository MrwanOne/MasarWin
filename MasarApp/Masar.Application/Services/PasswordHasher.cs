using System.Security.Cryptography;
using Masar.Application.Interfaces;

namespace Masar.Application.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public string HashPassword(string password, out string salt)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        salt = Convert.ToBase64String(saltBytes);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var hashBytes = pbkdf2.GetBytes(KeySize);
        return Convert.ToBase64String(hashBytes);
    }

    public bool Verify(string password, string passwordHash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256);
        var hashBytes = pbkdf2.GetBytes(KeySize);
        var storedBytes = Convert.FromBase64String(passwordHash);
        return CryptographicOperations.FixedTimeEquals(hashBytes, storedBytes);
    }
}
