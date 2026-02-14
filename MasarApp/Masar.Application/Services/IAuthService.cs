using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IAuthService
{
    Task<Result<UserDto>> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}
