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

public class StudentServiceV2 : IStudentService
{
    private readonly IStudentRepository _students;
    private readonly IDepartmentRepository _departments;
    private readonly ITeamRepository _teams;
    private readonly ICurrentUserService _currentUser;

    public StudentServiceV2(
        IStudentRepository students,
        IDepartmentRepository departments,
        ITeamRepository teams,
        ICurrentUserService currentUser)
    {
        _students = students;
        _departments = departments;
        _teams = teams;
        _currentUser = currentUser;
    }

    public async Task<List<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _students.GetWithDetailsAsync(cancellationToken);
        return list.Select(s => s.ToDto()).ToList();
    }

    public async Task<StudentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = await _students.GetByIdAsync(id, cancellationToken);
        return student?.ToDto();
    }

    public async Task<StudentDto?> FindByFullNameAsync(string fullName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return null;
        
        var students = await _students.GetWithDetailsAsync(cancellationToken);
        var student = students.FirstOrDefault(s => s.FullName.Equals(fullName.Trim(), StringComparison.OrdinalIgnoreCase));
        return student?.ToDto();
    }

    public async Task<Result<StudentDto>> AddAsync(StudentDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<StudentDto>.Failure(authCheck.Message);

        if (string.IsNullOrWhiteSpace(dto.FullName)) return Result<StudentDto>.Failure("Full name is required.");
        if (string.IsNullOrWhiteSpace(dto.StudentNumber)) return Result<StudentDto>.Failure("Student number is required.");
        
        var existing = await _students.GetByStudentNumberAsync(dto.StudentNumber, cancellationToken);
        if (existing != null) return Result<StudentDto>.Failure("A student with this number already exists.");

        var entity = new Student
        {
            StudentNumber = dto.StudentNumber.Trim(),
            FullName = dto.FullName.Trim(),
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim() ?? string.Empty,
            DepartmentId = dto.DepartmentId,
            TeamId = dto.TeamId == 0 ? null : dto.TeamId,
            EnrollmentYear = dto.EnrollmentYear,
            Gender = dto.Gender?.Trim() ?? string.Empty,
            GPA = dto.GPA,
            Level = dto.Level,
            Status = dto.Status
        };

        await _students.AddAsync(entity, cancellationToken);
        var created = await GetByIdAsync(entity.StudentId, cancellationToken);
        return Result<StudentDto>.Success(created ?? entity.ToDto());
    }

    public async Task<Result<StudentDto>> UpdateAsync(StudentDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<StudentDto>.Failure(authCheck.Message);

        var entity = await _students.GetByIdAsync(dto.StudentId, cancellationToken);
        if (entity == null) return Result<StudentDto>.Failure("Student not found.");

        if (string.IsNullOrWhiteSpace(dto.StudentNumber)) return Result<StudentDto>.Failure("Student number is required.");
        var existing = await _students.GetByStudentNumberAsync(dto.StudentNumber, cancellationToken);
        if (existing != null && existing.StudentId != dto.StudentId) 
            return Result<StudentDto>.Failure("A student with this number already exists.");

        entity.StudentNumber = dto.StudentNumber.Trim();
        entity.FullName = dto.FullName.Trim();
        entity.Email = dto.Email?.Trim();
        entity.Phone = dto.Phone?.Trim() ?? string.Empty;
        entity.DepartmentId = dto.DepartmentId;
        entity.TeamId = dto.TeamId == 0 ? null : dto.TeamId;
        entity.EnrollmentYear = dto.EnrollmentYear;
        entity.Gender = dto.Gender?.Trim() ?? string.Empty;
        entity.GPA = dto.GPA;
        entity.Level = dto.Level;
        entity.Status = dto.Status;

        await _students.UpdateAsync(entity, cancellationToken);
        var updated = await GetByIdAsync(entity.StudentId, cancellationToken);
        return Result<StudentDto>.Success(updated ?? entity.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return authCheck;

        var entity = await _students.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result.Failure("Student not found.");

        await _students.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<StudentDto>> AssignTeamAsync(int studentId, int? teamId, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment, UserRole.Supervisor);
        if (authCheck.IsFailure) return Result<StudentDto>.Failure(authCheck.Message);

        var entity = await _students.GetByIdAsync(studentId, cancellationToken);
        if (entity == null) return Result<StudentDto>.Failure("Student not found.");
        
        entity.TeamId = teamId == 0 ? null : teamId;
        await _students.UpdateAsync(entity, cancellationToken);
        
        var updated = await GetByIdAsync(entity.StudentId, cancellationToken);
        return Result<StudentDto>.Success(updated ?? entity.ToDto());
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
