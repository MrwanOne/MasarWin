using Masar.Application.Common;
using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly ITeamRepository _teams;
    private readonly IDepartmentRepository _departments;
    private readonly IDoctorRepository _doctors;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectStateMachine _stateMachine;
    private readonly IProjectStatusHistoryRepository _statusHistory;
    private readonly IValidator<ProjectDto> _validator;

    public ProjectService(
        IProjectRepository projects,
        ITeamRepository teams,
        IDepartmentRepository departments,
        IDoctorRepository doctors,
        ICurrentUserService currentUser,
        IProjectStateMachine stateMachine,
        IProjectStatusHistoryRepository statusHistory,
        IValidator<ProjectDto> validator)
    {
        _projects = projects;
        _teams = teams;
        _departments = departments;
        _doctors = doctors;
        _currentUser = currentUser;
        _stateMachine = stateMachine;
        _statusHistory = statusHistory;
        _validator = validator;
    }

    public async Task<List<ProjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _projects.GetWithDetailsAsync(cancellationToken);
        return list.Select(p => p.ToDto()).ToList();
    }

    public async Task<ProjectDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken);
        return project?.ToDto();
    }

    public async Task<Result<ProjectDto>> AddAsync(ProjectDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<ProjectDto>.Failure(authCheck.Message);

        var validation = await ValidateAsync(dto, cancellationToken, isUpdate: false);
        if (validation.IsFailure) return Result<ProjectDto>.Failure(validation.Message);

        var entity = new Project
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Beneficiary = dto.Beneficiary?.Trim() ?? string.Empty,
            Status = dto.Status == 0 ? ProjectStatus.Proposed : dto.Status,
            CompletionRate = ClampCompletion(dto.CompletionRate),
            ProposedAt = dto.ProposedAt == default ? DateTime.UtcNow : dto.ProposedAt,
            ApprovedAt = dto.ApprovedAt,
            RejectionReason = dto.RejectionReason ?? string.Empty,
            DepartmentId = dto.DepartmentId,
            TeamId = dto.TeamId,
            SupervisorId = dto.SupervisorId
        };

        await _projects.AddAsync(entity, cancellationToken);
        var created = await GetByIdAsync(entity.ProjectId, cancellationToken);
        return Result<ProjectDto>.Success(created ?? entity.ToDto());
    }

    public async Task<Result<ProjectDto>> UpdateAsync(ProjectDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment, UserRole.Supervisor);
        if (authCheck.IsFailure) return Result<ProjectDto>.Failure(authCheck.Message);

        var entity = await _projects.GetByIdAsync(dto.ProjectId, cancellationToken);
        if (entity == null) return Result<ProjectDto>.Failure("Project not found.");

        // التحقق من صلاحية انتقال الحالة باستخدام آلة الحالة
        if (entity.Status != dto.Status)
        {
            var userRole = _currentUser.Role ?? UserRole.Student;
            var transitionResult = _stateMachine.TryTransition(entity.Status, dto.Status, userRole);
            if (transitionResult.IsFailure)
                return Result<ProjectDto>.Failure(transitionResult.Message);
        }

        var validation = await ValidateAsync(dto, cancellationToken, isUpdate: true, entity.ProjectId);
        if (validation.IsFailure) return Result<ProjectDto>.Failure(validation.Message);

        // تحديث الطوابع الزمنية حسب الحالة
        var oldStatus = entity.Status;
        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description?.Trim() ?? string.Empty;
        entity.Beneficiary = dto.Beneficiary?.Trim() ?? string.Empty;
        entity.Status = dto.Status;
        entity.CompletionRate = ClampCompletion(dto.CompletionRate);
        entity.DepartmentId = dto.DepartmentId;
        entity.TeamId = dto.TeamId;
        entity.SupervisorId = dto.SupervisorId;
        entity.RejectionReason = dto.RejectionReason ?? string.Empty;

        // تعيين الطوابع الزمنية للمراحل وتسجيل السجل
        if (oldStatus != dto.Status)
        {
            if (dto.Status == ProjectStatus.Approved && entity.ApprovedAt == null)
                entity.ApprovedAt = DateTime.UtcNow;
            if (dto.Status == ProjectStatus.Rejected)
                entity.ApprovedAt = null;

            await _statusHistory.AddAsync(new ProjectStatusHistory
            {
                ProjectId = entity.ProjectId,
                OldStatus = oldStatus,
                NewStatus = dto.Status,
                ChangedByUserId = _currentUser.UserId,
                ChangeReason = dto.StatusChangeReason ?? "Status updated via management interface"
            }, cancellationToken);
        }

        await _projects.UpdateAsync(entity, cancellationToken);
        var updated = await GetByIdAsync(entity.ProjectId, cancellationToken);
        return Result<ProjectDto>.Success(updated ?? entity.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return authCheck;

        var entity = await _projects.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result.Failure("Project not found.");

        await _projects.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<ProjectDto>> AcceptAsync(int id, int? supervisorId, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<ProjectDto>.Failure(authCheck.Message);

        var entity = await _projects.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result<ProjectDto>.Failure("Project not found.");

        // التحقق من صلاحية الانتقال باستخدام آلة الحالة
        var userRole = _currentUser.Role ?? UserRole.Student;
        var transitionResult = _stateMachine.TryTransition(entity.Status, ProjectStatus.Approved, userRole);
        if (transitionResult.IsFailure)
            return Result<ProjectDto>.Failure(transitionResult.Message);

        if (supervisorId.HasValue)
        {
            var supCheck = await EnsureSupervisorMatchesDepartment(supervisorId.Value, entity.DepartmentId, cancellationToken);
            if (supCheck.IsFailure) return Result<ProjectDto>.Failure(supCheck.Message);
            entity.SupervisorId = supervisorId;
        }

        var oldStatus = entity.Status;
        entity.Status = ProjectStatus.Approved;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.RejectionReason = string.Empty;

        await _statusHistory.AddAsync(new ProjectStatusHistory
        {
            ProjectId = entity.ProjectId,
            OldStatus = oldStatus,
            NewStatus = ProjectStatus.Approved,
            ChangedByUserId = _currentUser.UserId,
            ChangeReason = "Project accepted and supervisor assigned"
        }, cancellationToken);

        await _projects.UpdateAsync(entity, cancellationToken);
        return Result<ProjectDto>.Success(entity.ToDto());
    }

    public async Task<Result<ProjectDto>> RejectAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<ProjectDto>.Failure(authCheck.Message);

        var entity = await _projects.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result<ProjectDto>.Failure("Project not found.");

        // التحقق من صلاحية الانتقال باستخدام آلة الحالة
        var userRole = _currentUser.Role ?? UserRole.Student;
        var transitionResult = _stateMachine.TryTransition(entity.Status, ProjectStatus.Rejected, userRole);
        if (transitionResult.IsFailure)
            return Result<ProjectDto>.Failure(transitionResult.Message);

        var oldStatus = entity.Status;
        entity.Status = ProjectStatus.Rejected;
        entity.RejectionReason = reason?.Trim() ?? "Rejected";
        entity.ApprovedAt = null;

        await _statusHistory.AddAsync(new ProjectStatusHistory
        {
            ProjectId = entity.ProjectId,
            OldStatus = oldStatus,
            NewStatus = ProjectStatus.Rejected,
            ChangedByUserId = _currentUser.UserId,
            ChangeReason = reason ?? "Project rejected"
        }, cancellationToken);

        await _projects.UpdateAsync(entity, cancellationToken);
        return Result<ProjectDto>.Success(entity.ToDto());
    }

    public async Task<Result<ProjectDto>> AssignSupervisorAsync(int id, int supervisorId, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<ProjectDto>.Failure(authCheck.Message);

        var entity = await _projects.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result<ProjectDto>.Failure("Project not found.");

        var supCheck = await EnsureSupervisorMatchesDepartment(supervisorId, entity.DepartmentId, cancellationToken);
        if (supCheck.IsFailure) return Result<ProjectDto>.Failure(supCheck.Message);

        entity.SupervisorId = supervisorId;
        await _projects.UpdateAsync(entity, cancellationToken);
        return Result<ProjectDto>.Success(entity.ToDto());
    }

    public async Task<Result<ProjectDto>> SetCompletionRateAsync(int id, decimal completionRate, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment, UserRole.Supervisor);
        if (authCheck.IsFailure) return Result<ProjectDto>.Failure(authCheck.Message);

        var entity = await _projects.GetByIdAsync(id, cancellationToken);
        if (entity == null) return Result<ProjectDto>.Failure("Project not found.");

        entity.CompletionRate = ClampCompletion(completionRate);
        if (entity.CompletionRate >= 100)
        {
            entity.Status = ProjectStatus.Completed;
        }
        else if (entity.Status == ProjectStatus.Completed)
        {
            entity.Status = ProjectStatus.InProgress;
        }

        await _projects.UpdateAsync(entity, cancellationToken);
        return Result<ProjectDto>.Success(entity.ToDto());
    }

    private async Task<Result> ValidateAsync(ProjectDto dto, CancellationToken cancellationToken, bool isUpdate, int? projectId = null)
    {
        // 1. FluentValidation (المصادقة الهيكلية)
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(errors);
        }

        // 2. Business Validation (المصادقة المنطقية)
        var normalizedTitle = dto.Title.Trim();
        var existingTitle = await _projects.GetByTitleAsync(normalizedTitle, cancellationToken);
        if (existingTitle != null && (!isUpdate || existingTitle.ProjectId != projectId))
        {
            return Result.Failure("A project with the same title already exists.");
        }

        var deptCheck = await EnsureDepartmentExists(dto.DepartmentId, cancellationToken);
        if (deptCheck.IsFailure) return deptCheck;

        if (dto.TeamId.HasValue)
        {
            var existingTeamProject = await _projects.GetByTeamIdAsync(dto.TeamId.Value, cancellationToken);
            if (existingTeamProject != null && (!isUpdate || existingTeamProject.ProjectId != projectId))
            {
                return Result.Failure("This team already has a project assigned.");
            }

            var team = await _teams.GetWithDetailsAsync(dto.TeamId.Value, cancellationToken);
            if (team == null) return Result.Failure("Team not found.");
            if (team.DepartmentId != dto.DepartmentId)
            {
                return Result.Failure("Team must belong to the selected department.");
            }
        }

        if (dto.SupervisorId.HasValue)
        {
            var supCheck = await EnsureSupervisorMatchesDepartment(dto.SupervisorId.Value, dto.DepartmentId, cancellationToken);
            if (supCheck.IsFailure) return Result.Failure(supCheck.Message);
        }

        return Result.Success();
    }

    private async Task<Result> EnsureDepartmentExists(int departmentId, CancellationToken cancellationToken)
    {
        var dept = await _departments.GetByIdAsync(departmentId, cancellationToken);
        return dept == null ? Result.Failure("Department not found.") : Result.Success();
    }

    private async Task<Result> EnsureSupervisorMatchesDepartment(int supervisorId, int departmentId, CancellationToken cancellationToken)
    {
        var supervisor = await _doctors.GetByIdAsync(supervisorId, cancellationToken);
        if (supervisor == null) return Result.Failure("Supervisor not found.");
        if (supervisor.DepartmentId != departmentId)
        {
            return Result.Failure("Supervisor must belong to the selected department.");
        }
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

    private static decimal ClampCompletion(decimal value)
    {
        if (value < 0) return 0;
        if (value > 100) return 100;
        return value;
    }
}
