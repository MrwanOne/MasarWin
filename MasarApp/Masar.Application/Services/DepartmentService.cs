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

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departments;
    private readonly ICollegeRepository _colleges;
    private readonly IDoctorRepository _doctors;
    private readonly ICurrentUserService _currentUser;

    public DepartmentService(
        IDepartmentRepository departments,
        ICollegeRepository colleges,
        IDoctorRepository doctors,
        ICurrentUserService currentUser)
    {
        _departments = departments;
        _colleges = colleges;
        _doctors = doctors;
        _currentUser = currentUser;
    }

    public async Task<List<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _departments.GetWithCollegeAsync(cancellationToken);
        return list.Select(d => d.ToDto()).ToList();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _departments.GetWithCollegeAsync(id, cancellationToken);
        return entity?.ToDto();
    }

    public async Task<Result<DepartmentDto>> AddAsync(DepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<DepartmentDto>.Failure(authCheck.Message);

        var normResult = NormalizeNames(dto);
        if (normResult.IsFailure) return Result<DepartmentDto>.Failure(normResult.Message);
        var (nameAr, nameEn) = normResult.Value;

        var collegeCheck = await EnsureCollegeExists(dto.CollegeId, cancellationToken);
        if (collegeCheck.IsFailure) return Result<DepartmentDto>.Failure(collegeCheck.Message);

        var existing = await _departments.GetByNamesAsync(nameAr, nameEn, dto.CollegeId, cancellationToken);
        if (existing != null)
        {
            return Result<DepartmentDto>.Failure("اسم القسم موجود بالفعل في نفس الكلية. / A department with the same name already exists in this college.");
        }

        var entity = new Domain.Entities.Department
        {
            NameAr = nameAr,
            NameEn = nameEn,
            Code = dto.Code?.Trim() ?? string.Empty,
            CollegeId = dto.CollegeId,
            HeadOfDepartmentId = dto.HeadOfDepartmentId,
            CreatedAt = DateTime.UtcNow
        };

        await _departments.AddAsync(entity, cancellationToken);
        var created = await _departments.GetWithCollegeAsync(entity.DepartmentId, cancellationToken);
        return Result<DepartmentDto>.Success((created ?? entity).ToDto());
    }

    public async Task<Result<DepartmentDto>> UpdateAsync(DepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<DepartmentDto>.Failure(authCheck.Message);

        var normResult = NormalizeNames(dto);
        if (normResult.IsFailure) return Result<DepartmentDto>.Failure(normResult.Message);
        var (nameAr, nameEn) = normResult.Value;

        var collegeCheck = await EnsureCollegeExists(dto.CollegeId, cancellationToken);
        if (collegeCheck.IsFailure) return Result<DepartmentDto>.Failure(collegeCheck.Message);

        var entity = await _departments.GetWithCollegeAsync(dto.DepartmentId, cancellationToken);
        if (entity == null) return Result<DepartmentDto>.Failure("القسم غير موجود. / Department not found.");

        var duplicate = await _departments.GetByNamesAsync(nameAr, nameEn, dto.CollegeId, cancellationToken);
        if (duplicate != null && duplicate.DepartmentId != entity.DepartmentId)
        {
            return Result<DepartmentDto>.Failure("اسم القسم موجود بالفعل في نفس الكلية. / A department with the same name already exists in this college.");
        }

        entity.NameAr = nameAr;
        entity.NameEn = nameEn;
        entity.Code = dto.Code?.Trim() ?? string.Empty;
        entity.CollegeId = dto.CollegeId;

        if (dto.HeadOfDepartmentId.HasValue)
        {
            var head = await _doctors.GetWithDepartmentAsync(dto.HeadOfDepartmentId.Value, cancellationToken);
            if (head == null) return Result<DepartmentDto>.Failure("رئيس القسم غير موجود. / Head of department not found.");
            if (head.DepartmentId != entity.DepartmentId)
            {
                return Result<DepartmentDto>.Failure("يجب أن ينتمي رئيس القسم إلى نفس القسم. / Head of department must belong to the same department.");
            }
            entity.HeadOfDepartmentId = dto.HeadOfDepartmentId;
        }
        else
        {
            entity.HeadOfDepartmentId = null;
        }

        await _departments.UpdateAsync(entity, cancellationToken);
        var updated = await _departments.GetWithCollegeAsync(entity.DepartmentId, cancellationToken);
        return Result<DepartmentDto>.Success((updated ?? entity).ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return authCheck;

        var entity = await _departments.GetWithCollegeAsync(id, cancellationToken);
        if (entity == null) return Result.Failure("القسم غير موجود. / Department not found.");

        await _departments.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SetHeadOfDepartmentAsync(int departmentId, int doctorId, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return authCheck;

        var department = await _departments.GetWithCollegeAsync(departmentId, cancellationToken);
        if (department == null) return Result.Failure("القسم غير موجود. / Department not found.");

        var doctor = await _doctors.GetWithDepartmentAsync(doctorId, cancellationToken);
        if (doctor == null) return Result.Failure("الدكتور غير موجود. / Doctor not found.");

        // Check if doctor belongs to the same college as the department
        if (doctor.CollegeId != department.CollegeId)
        {
            return Result.Failure("الدكتور يجب أن يكون من نفس الكلية. / Doctor must belong to the same college.");
        }

        // Use repository method for atomic operation
        var success = await _departments.SetHeadOfDepartmentInternalAsync(departmentId, doctorId, cancellationToken);
        
        return success ? Result.Success() : Result.Failure("فشل تعيين رئيس القسم / Failed to set HOD");
    }

    public async Task<Result> ClearHeadOfDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin);
        if (authCheck.IsFailure) return authCheck;

        var department = await _departments.GetByIdAsync(departmentId, cancellationToken);
        if (department == null) return Result.Failure("القسم غير موجود. / Department not found.");

        department.HeadOfDepartmentId = null;
        await _departments.UpdateAsync(department, cancellationToken);
        return Result.Success();
    }

    private static Result<(string nameAr, string nameEn)> NormalizeNames(DepartmentDto dto)
    {
        var nameAr = dto.NameAr?.Trim() ?? string.Empty;
        var nameEn = string.IsNullOrWhiteSpace(dto.NameEn) ? nameAr : dto.NameEn.Trim();

        if (string.IsNullOrWhiteSpace(nameAr))
        {
            return Result<(string, string)>.Failure("اسم القسم بالعربية مطلوب. / Department Arabic name is required.");
        }

        return Result<(string, string)>.Success((nameAr, nameEn));
    }

    private async Task<Result> EnsureCollegeExists(int collegeId, CancellationToken cancellationToken)
    {
        var college = await _colleges.GetByIdAsync(collegeId, cancellationToken);
        if (college == null) return Result.Failure("الكلية غير موجودة. / College not found.");
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
