using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;

    public UserService(IUserRepository users, IPasswordHasher passwordHasher, ICurrentUserService currentUser)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _users.GetWithDetailsAsync(cancellationToken);
        return list.Select(u => u.ToDto()).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        return user?.ToDto();
    }

    public async Task<Result<UserDto>> AddAsync(UserDto dto, string password, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin);
        if (authCheck.IsFailure) return Result<UserDto>.Failure(authCheck.Message);

        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(password))
        {
            return Result<UserDto>.Failure("اسم المستخدم وكلمة المرور مطلوبان. / Username and password are required.");
        }

        var existing = await _users.GetByUsernameAsync(dto.Username.Trim(), cancellationToken);
        if (existing != null)
        {
            return Result<UserDto>.Failure("يوجد مستخدم بنفس اسم المستخدم مسبقاً. / A user with the same username already exists.");
        }

        var hash = _passwordHasher.HashPassword(password, out var salt);
        var entity = new User
        {
            Username = dto.Username.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = dto.Role,
            IsActive = dto.IsActive,
            DoctorId = dto.DoctorId,
            StudentId = dto.StudentId,
            CreatedAt = DateTime.UtcNow
        };

        await _users.AddAsync(entity, cancellationToken);
        return Result<UserDto>.Success(entity.ToDto());
    }

    public async Task<Result<UserDto>> UpdateAsync(UserDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin);
        if (authCheck.IsFailure) return Result<UserDto>.Failure(authCheck.Message);

        var entity = await _users.GetByIdAsync(dto.UserId, cancellationToken);
        if (entity == null) return Result<UserDto>.Failure("المستخدم غير موجود. / User not found.");

        var normalizedUsername = dto.Username?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return Result<UserDto>.Failure("اسم المستخدم مطلوب. / Username is required.");
        }

        if (!string.Equals(entity.Username, normalizedUsername, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _users.GetByUsernameAsync(normalizedUsername, cancellationToken);
            if (existing != null && existing.UserId != entity.UserId)
            {
                return Result<UserDto>.Failure("يوجد مستخدم بنفس اسم المستخدم مسبقاً. / A user with the same username already exists.");
            }
        }

        entity.Username = normalizedUsername;
        entity.Role = dto.Role;
        entity.IsActive = dto.IsActive;
        entity.DoctorId = dto.DoctorId;
        entity.StudentId = dto.StudentId;

        await _users.UpdateAsync(entity, cancellationToken);
        return Result<UserDto>.Success(entity.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin);
        if (authCheck.IsFailure) return authCheck;

        var entity = await _users.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result.Failure("المستخدم غير موجود. / User not found.");

        await _users.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<UserDto>> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin);
        if (authCheck.IsFailure) return Result<UserDto>.Failure(authCheck.Message);

        var entity = await _users.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result<UserDto>.Failure("المستخدم غير موجود. / User not found.");

        entity.IsActive = isActive;
        await _users.UpdateAsync(entity, cancellationToken);
        return Result<UserDto>.Success(entity.ToDto());
    }

    public async Task<Result<UserDto>> ResetPasswordAsync(int id, string newPassword, CancellationToken cancellationToken = default)
    {
        if (_currentUser.Role != UserRole.Admin && _currentUser.UserId != id)
        {
            return Result<UserDto>.Failure("ليس لديك صلاحية إعادة تعيين كلمة المرور هذه. / You are not authorized to reset this password.");
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return Result<UserDto>.Failure("كلمة المرور الجديدة مطلوبة. / New password is required.");
        }

        var entity = await _users.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result<UserDto>.Failure("المستخدم غير موجود. / User not found.");

        entity.PasswordHash = _passwordHasher.HashPassword(newPassword, out var salt);
        entity.PasswordSalt = salt;

        await _users.UpdateAsync(entity, cancellationToken);
        return Result<UserDto>.Success(entity.ToDto());
    }

    private Result EnsureAuthorized(params UserRole[] allowedRoles)
    {
        if (!_currentUser.IsAuthenticated) return Result.Failure("المستخدم غير مسجل الدخول. / User is not authenticated.");
        if (allowedRoles.Any() && !allowedRoles.Contains(_currentUser.Role ?? UserRole.Student))
        {
            return Result.Failure("ليس لديك صلاحية لتنفيذ هذه العملية. / User is not authorized to perform this operation.");
        }
        return Result.Success();
    }
}
