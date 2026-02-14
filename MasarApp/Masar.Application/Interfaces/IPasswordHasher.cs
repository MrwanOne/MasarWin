namespace Masar.Application.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password, out string salt);
    bool Verify(string password, string passwordHash, string salt);
}
