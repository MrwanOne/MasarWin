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

public class DoctorServiceV2 : IDoctorService
{
    private readonly IDoctorRepository _doctors;
    private readonly IDepartmentRepository _departments;
    private readonly ICurrentUserService _currentUser;

    public DoctorServiceV2(
        IDoctorRepository doctors,
        IDepartmentRepository departments,
        ICurrentUserService currentUser)
    {
        _doctors = doctors;
        _departments = departments;
        _currentUser = currentUser;
    }

    public async Task<List<DoctorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _doctors.GetWithDepartmentAsync(cancellationToken);
        return list.Select(d => d.ToDto()).ToList();
    }

    public async Task<DoctorDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var doctor = await _doctors.GetByIdAsync(id, cancellationToken);
        return doctor?.ToDto();
    }

    public async Task<DoctorDto?> FindByFullNameAsync(string fullName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return null;
        
        var doctors = await _doctors.GetWithDepartmentAsync(cancellationToken);
        var doctor = doctors.FirstOrDefault(d => d.FullName.Equals(fullName.Trim(), StringComparison.OrdinalIgnoreCase));
        return doctor?.ToDto();
    }

    public async Task<Result<DoctorDto>> AddAsync(DoctorDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<DoctorDto>.Failure(authCheck.Message);

        if (string.IsNullOrWhiteSpace(dto.FullName)) return Result<DoctorDto>.Failure("Full name is required.");
        
        var entity = new Doctor
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim() ?? string.Empty,
            Qualification = dto.Qualification?.Trim() ?? string.Empty,
            Gender = dto.Gender?.Trim() ?? string.Empty,
            CollegeId = dto.CollegeId,
            DepartmentId = dto.DepartmentId,
            Rank = Enum.TryParse<AcademicRank>(dto.Rank, out var rank) ? rank : AcademicRank.Lecturer
        };

        await _doctors.AddAsync(entity, cancellationToken);
        var created = await GetByIdAsync(entity.DoctorId, cancellationToken);
        return Result<DoctorDto>.Success(created ?? entity.ToDto());
    }

    public async Task<Result<DoctorDto>> UpdateAsync(DoctorDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<DoctorDto>.Failure(authCheck.Message);

        var entity = await _doctors.GetByIdAsync(dto.DoctorId, cancellationToken);
        if (entity == null) return Result<DoctorDto>.Failure("Doctor not found.");

        entity.FullName = dto.FullName.Trim();
        entity.Email = dto.Email?.Trim();
        entity.Phone = dto.Phone?.Trim() ?? string.Empty;
        entity.Qualification = dto.Qualification?.Trim() ?? string.Empty;
        entity.Gender = dto.Gender?.Trim() ?? string.Empty;
        entity.CollegeId = dto.CollegeId;
        entity.DepartmentId = dto.DepartmentId;
        entity.Rank = Enum.TryParse<AcademicRank>(dto.Rank, out var rank) ? rank : AcademicRank.Lecturer;

        await _doctors.UpdateAsync(entity, cancellationToken);
        var updated = await GetByIdAsync(entity.DoctorId, cancellationToken);
        return Result<DoctorDto>.Success(updated ?? entity.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return authCheck;

        var entity = await _doctors.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result.Failure("Doctor not found.");

        await _doctors.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }
    private Result EnsureAuthorized(params UserRole[] allowedRoles)
    {
        if (!_currentUser.IsAuthenticated) return Result.Failure("User is not authenticated.");
        if (allowedRoles.Any() && !allowedRoles.Contains(_currentUser.Role ?? UserRole.Student))
        {
            return Result.Failure("User is not authorized to perform this operation.");
        }
        return Result.Success();
    }
}
