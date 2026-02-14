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

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teams;
    private readonly ICurrentUserService _currentUser;

    public TeamService(ITeamRepository teams, ICurrentUserService currentUser)
    {
        _teams = teams;
        _currentUser = currentUser;
    }

    public async Task<List<TeamDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _teams.GetWithDetailsAsync(cancellationToken);
        return list.Select(t => t.ToDto()).ToList();
    }

    public async Task<TeamDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var team = await _teams.GetByIdAsync(id, cancellationToken);
        return team?.ToDto();
    }

    public async Task<Result<TeamDto>> AddAsync(TeamDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<TeamDto>.Failure(authCheck.Message);

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Result<TeamDto>.Failure("Team name is required.");
        }

        var entity = new Team
        {
            Name = dto.Name.Trim(),
            DepartmentId = dto.DepartmentId,
            SupervisorId = dto.SupervisorId,
            CommitteeId = dto.CommitteeId,
            CreatedAt = DateTime.UtcNow
        };

        await _teams.AddAsync(entity, cancellationToken);
        var created = await _teams.GetByIdAsync(entity.TeamId, cancellationToken);
        return Result<TeamDto>.Success((created ?? entity).ToDto());
    }

    public async Task<Result<TeamDto>> UpdateAsync(TeamDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment, UserRole.Supervisor);
        if (authCheck.IsFailure) return Result<TeamDto>.Failure(authCheck.Message);

        var entity = await _teams.GetByIdAsync(dto.TeamId, cancellationToken);
        if (entity == null)
        {
            return Result<TeamDto>.Failure("Team not found.");
        }

        entity.Name = dto.Name.Trim();
        entity.DepartmentId = dto.DepartmentId;
        entity.SupervisorId = dto.SupervisorId;
        entity.CommitteeId = dto.CommitteeId;

        await _teams.UpdateAsync(entity, cancellationToken);
        var updated = await _teams.GetByIdAsync(entity.TeamId, cancellationToken);
        return Result<TeamDto>.Success((updated ?? entity).ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return authCheck;

        var entity = await _teams.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return Result.Failure("Team not found.");
        }

        await _teams.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<TeamDto>> AssignSupervisorAsync(int teamId, int? supervisorId, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<TeamDto>.Failure(authCheck.Message);

        var entity = await _teams.GetByIdAsync(teamId, cancellationToken);
        if (entity == null) return Result<TeamDto>.Failure("Team not found.");

        entity.SupervisorId = supervisorId;
        await _teams.UpdateAsync(entity, cancellationToken);
        return Result<TeamDto>.Success(entity.ToDto());
    }

    public async Task<Result<TeamDto>> AssignCommitteeAsync(int teamId, int? committeeId, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return Result<TeamDto>.Failure(authCheck.Message);

        var entity = await _teams.GetByIdAsync(teamId, cancellationToken);
        if (entity == null) return Result<TeamDto>.Failure("Team not found.");

        entity.CommitteeId = committeeId;
        await _teams.UpdateAsync(entity, cancellationToken);
        return Result<TeamDto>.Success(entity.ToDto());
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
