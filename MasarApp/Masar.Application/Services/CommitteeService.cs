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

public class CommitteeService : ICommitteeService
{
    private readonly ICommitteeRepository _committees;
    private readonly IDepartmentRepository _departments;
    private readonly IDoctorRepository _doctors;
    private readonly ITeamRepository _teams;
    private readonly ICurrentUserService _currentUser;

    public CommitteeService(
        ICommitteeRepository committees,
        IDepartmentRepository departments,
        IDoctorRepository doctors,
        ITeamRepository teams,
        ICurrentUserService currentUser)
    {
        _committees = committees;
        _departments = departments;
        _doctors = doctors;
        _teams = teams;
        _currentUser = currentUser;
    }

    public async Task<List<CommitteeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _committees.GetWithMembersAsync(cancellationToken);
        return list.Select(c => c.ToDto()).ToList();
    }

    public async Task<CommitteeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var committee = await _committees.GetByIdAsync(id, cancellationToken);
        return committee?.ToDto();
    }

    public async Task<Result<CommitteeDto>> AddAsync(CommitteeDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<CommitteeDto>.Failure(authCheck.Message);

        if (string.IsNullOrWhiteSpace(dto.Name)) return Result<CommitteeDto>.Failure("اسم اللجنة مطلوب. / Committee name is required.");

        // DepartmentId is now optional - committees can be associated with College only
        if (dto.DepartmentId > 0)
        {
            var deptCheck = await EnsureDepartmentExists(dto.DepartmentId, cancellationToken);
            if (deptCheck.IsFailure) return Result<CommitteeDto>.Failure(deptCheck.Message);
        }

        var entity = new Committee
        {
            Name = dto.Name.Trim(),
            DepartmentId = dto.DepartmentId > 0 ? dto.DepartmentId : null,
            TermId = dto.TermId,
            CreatedAt = DateTime.UtcNow
        };

        await _committees.AddAsync(entity, cancellationToken);
        var created = await _committees.GetByIdAsync(entity.CommitteeId, cancellationToken);
        return Result<CommitteeDto>.Success((created ?? entity).ToDto());
    }

    public async Task<Result<CommitteeDto>> UpdateAsync(CommitteeDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<CommitteeDto>.Failure(authCheck.Message);

        if (string.IsNullOrWhiteSpace(dto.Name)) return Result<CommitteeDto>.Failure("اسم اللجنة مطلوب. / Committee name is required.");

        var entity = await _committees.GetByIdAsync(dto.CommitteeId, cancellationToken);
        if (entity == null) return Result<CommitteeDto>.Failure("اللجنة غير موجودة. / Committee not found.");

        // DepartmentId is now optional - committees can be associated with College only
        if (dto.DepartmentId > 0)
        {
            var deptCheck = await EnsureDepartmentExists(dto.DepartmentId, cancellationToken);
            if (deptCheck.IsFailure) return Result<CommitteeDto>.Failure(deptCheck.Message);
        }

        entity.Name = dto.Name.Trim();
        entity.DepartmentId = dto.DepartmentId > 0 ? dto.DepartmentId : null;
        entity.TermId = dto.TermId;

        await _committees.UpdateAsync(entity, cancellationToken);
        var updated = await _committees.GetByIdAsync(entity.CommitteeId, cancellationToken);
        return Result<CommitteeDto>.Success((updated ?? entity).ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return authCheck;

        var entity = await _committees.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result.Failure("اللجنة غير موجودة. / Committee not found.");

        await _committees.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<CommitteeDto>> AssignDoctorAsync(int committeeId, int doctorId, bool isChair, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<CommitteeDto>.Failure(authCheck.Message);

        var committee = await _committees.GetWithMembersAsync(committeeId, cancellationToken);
        if (committee == null) return Result<CommitteeDto>.Failure("اللجنة غير موجودة. / Committee not found.");

        var doctor = await _doctors.GetWithDepartmentAsync(doctorId, cancellationToken);
        if (doctor == null) return Result<CommitteeDto>.Failure("الدكتور غير موجود. / Doctor not found.");

        var collegeCheck = await EnsureSameCollege(committee, doctor, cancellationToken);
        if (collegeCheck.IsFailure) return Result<CommitteeDto>.Failure(collegeCheck.Message);

        // Conflict of interest: supervisor cannot be on a committee that evaluates their own team
        var allTeams = await _teams.GetAllAsync(cancellationToken);
        var supervisedTeams = allTeams.Where(t => t.SupervisorId == doctorId && t.CommitteeId == committeeId).ToList();
        if (supervisedTeams.Any())
        {
            return Result<CommitteeDto>.Failure("لا يمكن إضافة المشرف كعضو في لجنة تقيّم فريقه (تعارض مصالح). / Supervisor cannot be a member of a committee evaluating their own team (conflict of interest).");
        }

        var hasCommitteeThisTerm = committee.TermId.HasValue && 
            await _committees.DoctorHasCommitteeInTermAsync(doctorId, committee.TermId.Value, committeeId, cancellationToken);
        if (hasCommitteeThisTerm)
        {
            return Result<CommitteeDto>.Failure("هذا الدكتور عضو في لجنة أخرى لنفس الفصل الدراسي. / This doctor already serves on a committee for the selected term.");
        }

        // Use dedicated repository methods instead of Update for proper tracking
        var existing = await _committees.GetMemberAsync(committeeId, doctorId, cancellationToken);
        var newRole = isChair ? CommitteeMemberRole.Chair : CommitteeMemberRole.Member;

        if (existing == null)
        {
            await _committees.AddMemberAsync(new CommitteeMember
            {
                CommitteeId = committeeId,
                DoctorId = doctorId,
                Role = newRole
            }, cancellationToken);
        }
        else if (existing.Role != newRole)
        {
            existing.Role = newRole;
            await _committees.UpdateMemberAsync(existing, cancellationToken);
        }

        var updated = await _committees.GetWithMembersAsync(committeeId, cancellationToken);
        return Result<CommitteeDto>.Success((updated ?? committee).ToDto());
    }

    public async Task<Result> RemoveDoctorAsync(int committeeId, int doctorId, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return authCheck;

        var committee = await _committees.GetWithMembersAsync(committeeId, cancellationToken);
        if (committee == null) return Result.Failure("اللجنة غير موجودة. / Committee not found.");

        await _committees.RemoveMemberAsync(committeeId, doctorId, cancellationToken);
        return Result.Success();
    }

    private async Task<Result> EnsureDepartmentExists(int departmentId, CancellationToken cancellationToken)
    {
        var department = await _departments.GetWithCollegeAsync(departmentId, cancellationToken);
        if (department == null) return Result.Failure("القسم غير موجود. / Department not found.");
        return Result.Success();
    }

    private async Task<Result> EnsureSameCollege(Committee committee, Doctor doctor, CancellationToken cancellationToken)
    {
        // If committee has no department, use doctor's college for comparison
        if (committee.DepartmentId == null)
        {
            // Committee without department - allow any doctor (will rely on college filtering in UI)
            return Result.Success();
        }

        var committeeDepartment = committee.Department ?? await _departments.GetWithCollegeAsync(committee.DepartmentId.Value, cancellationToken);
        if (committeeDepartment == null) return Result.Failure("القسم غير موجود. / Department not found.");

        if (doctor.DepartmentId != committeeDepartment.DepartmentId)
        {
            if (doctor.Department?.CollegeId != committeeDepartment.CollegeId)
            {
                return Result.Failure("يجب أن ينتمي الدكتور إلى نفس الكلية التي تتبع لها اللجنة. / Doctor must belong to the same college as the committee.");
            }
        }
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
