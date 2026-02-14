using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Result<UserDto>.Failure("Username and password are required.");
        }

        var user = await _users.GetByUsernameAsync(username.Trim(), cancellationToken);
        if (user == null)
        {
            return Result<UserDto>.Failure("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            return Result<UserDto>.Failure("User account is inactive.");
        }

        var valid = _passwordHasher.Verify(password, user.PasswordHash, user.PasswordSalt);
        if (!valid)
        {
            return Result<UserDto>.Failure("Invalid username or password.");
        }

        return Result<UserDto>.Success(user.ToDto());
    }
}
