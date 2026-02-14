using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> AddAsync(UserDto dto, string password, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateAsync(UserDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> ResetPasswordAsync(int id, string newPassword, CancellationToken cancellationToken = default);
}
