using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public class CollegeService : ICollegeService
{
    private readonly ICollegeRepository _colleges;
    private readonly ICurrentUserService _currentUser;

    public CollegeService(ICollegeRepository colleges, ICurrentUserService currentUser)
    {
        _colleges = colleges;
        _currentUser = currentUser;
    }

    public async Task<List<CollegeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _colleges.GetAllAsync(cancellationToken);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<Result<CollegeDto>> AddAsync(CollegeDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin);
        if (authCheck.IsFailure) return Result<CollegeDto>.Failure(authCheck.Message);

        var entity = new Domain.Entities.College
        {
            NameAr = dto.NameAr.Trim(),
            NameEn = dto.NameEn.Trim(),
            Code = dto.Code?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _colleges.AddAsync(entity, cancellationToken);
        return Result<CollegeDto>.Success(entity.ToDto());
    }

    public async Task<Result<CollegeDto>> UpdateAsync(CollegeDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin);
        if (authCheck.IsFailure) return Result<CollegeDto>.Failure(authCheck.Message);

        var entity = await _colleges.GetByIdAsync(dto.CollegeId, cancellationToken);
        if (entity == null) return Result<CollegeDto>.Failure("الكلية غير موجودة. / College not found.");

        entity.NameAr = dto.NameAr.Trim();
        entity.NameEn = dto.NameEn.Trim();
        entity.Code = dto.Code?.Trim() ?? string.Empty;

        await _colleges.UpdateAsync(entity, cancellationToken);
        return Result<CollegeDto>.Success(entity.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin);
        if (authCheck.IsFailure) return authCheck;

        var entity = await _colleges.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result.Failure("الكلية غير موجودة. / College not found.");

        await _colleges.DeleteAsync(entity, cancellationToken);
        return Result.Success();
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
