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

public class DiscussionService : IDiscussionService
{
    private readonly IDiscussionRepository _discussions;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectProcedureRepository _procedures;

    public DiscussionService(
        IDiscussionRepository discussions,
        ICurrentUserService currentUser,
        IProjectProcedureRepository procedures)
    {
        _discussions = discussions;
        _currentUser = currentUser;
        _procedures  = procedures;
    }

    public async Task<List<DiscussionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _discussions.GetWithDetailsAsync(cancellationToken);
        return list.Select(d => d.ToDto()).ToList();
    }

    public async Task<DiscussionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var discussion = await _discussions.GetByIdAsync(id, cancellationToken);
        return discussion?.ToDto();
    }

    public async Task<Result<DiscussionDto>> ScheduleAsync(DiscussionDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment, UserRole.Supervisor);
        if (authCheck.IsFailure) return Result<DiscussionDto>.Failure(authCheck.Message);

        var valResult = ValidateSchedule(dto);
        if (valResult.IsFailure) return Result<DiscussionDto>.Failure(valResult.Message);

        var conflictCheck = await EnsureNoConflicts(dto, null, cancellationToken);
        if (conflictCheck.IsFailure) return Result<DiscussionDto>.Failure(conflictCheck.Message);

        var entity = new Discussion
        {
            TeamId = dto.TeamId,
            CommitteeId = dto.CommitteeId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Place = dto.Place?.Trim() ?? string.Empty,
            SupervisorScore = dto.SupervisorScore,
            CommitteeScore = dto.CommitteeScore,
            FinalScore = dto.FinalScore,
            ReportText = dto.ReportText ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _discussions.AddAsync(entity, cancellationToken);
        var created = await _discussions.GetByIdAsync(entity.DiscussionId, cancellationToken);
        return Result<DiscussionDto>.Success((created ?? entity).ToDto());
    }

    public async Task<Result<DiscussionDto>> UpdateAsync(DiscussionDto dto, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment, UserRole.Supervisor);
        if (authCheck.IsFailure) return Result<DiscussionDto>.Failure(authCheck.Message);

        var entity = await _discussions.GetByIdAsync(dto.DiscussionId, cancellationToken);
        if (entity == null) return Result<DiscussionDto>.Failure("المناقشة غير موجودة. / Discussion not found.");

        var valResult = ValidateSchedule(dto);
        if (valResult.IsFailure) return Result<DiscussionDto>.Failure(valResult.Message);

        var conflictCheck = await EnsureNoConflicts(dto, entity.DiscussionId, cancellationToken);
        if (conflictCheck.IsFailure) return Result<DiscussionDto>.Failure(conflictCheck.Message);

        entity.TeamId = dto.TeamId;
        entity.CommitteeId = dto.CommitteeId;
        entity.StartTime = dto.StartTime;
        entity.EndTime = dto.EndTime;
        entity.Place = dto.Place?.Trim() ?? string.Empty;
        entity.SupervisorScore = dto.SupervisorScore;
        entity.CommitteeScore = dto.CommitteeScore;
        entity.FinalScore = dto.FinalScore;
        entity.ReportText = dto.ReportText ?? string.Empty;

        await _discussions.UpdateAsync(entity, cancellationToken);
        var updated = await _discussions.GetByIdAsync(entity.DiscussionId, cancellationToken);
        return Result<DiscussionDto>.Success((updated ?? entity).ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment);
        if (authCheck.IsFailure) return authCheck;

        var entity = await _discussions.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return Result.Failure("المناقشة غير موجودة. / Discussion not found.");
        }

        await _discussions.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<DiscussionDto>> SaveEvaluationAsync(
        int discussionId,
        decimal supervisorScore,
        decimal committeeScore,
        string reportText,
        CancellationToken cancellationToken = default)
    {
        var authCheck = EnsureAuthorized(UserRole.Admin, UserRole.HeadOfDepartment, UserRole.Supervisor);
        if (authCheck.IsFailure) return Result<DiscussionDto>.Failure(authCheck.Message);

        // تفويض كامل إلى SP_SAVE_EVALUATION:
        // حساب الدرجة النهائية + تحديث المناقشة + إنهاء المشروع تلقائياً — كله في Transaction واحدة
        var (code, msg) = await _procedures.SaveEvaluationAsync(
            discussionId, supervisorScore, committeeScore, reportText, cancellationToken);

        if (code != 0)
            return Result<DiscussionDto>.Failure(msg);

        var updated = await _discussions.GetByIdAsync(discussionId, cancellationToken);
        return updated is not null
            ? Result<DiscussionDto>.Success(updated.ToDto())
            : Result<DiscussionDto>.Failure("لم يتم إيجاد المناقشة بعد الحفظ. / Discussion not found after save.");
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

    private static Result ValidateSchedule(DiscussionDto dto)
    {
        if (dto.StartTime == default || dto.EndTime == default)
        {
            return Result.Failure("تاريخ ووقت المناقشة مطلوبان. / Discussion date and time are required.");
        }

        if (dto.EndTime <= dto.StartTime)
        {
            return Result.Failure("يجب أن يكون وقت انتهاء المناقشة بعد وقت بدايتها. / Discussion end time must be after the start time.");
        }

        if (string.IsNullOrWhiteSpace(dto.Place))
        {
            return Result.Failure("مكان المناقشة مطلوب. / Discussion place is required.");
        }

        return Result.Success();
    }

    private async Task<Result> EnsureNoConflicts(DiscussionDto dto, int? currentId, CancellationToken cancellationToken)
    {
        var conflicts = await _discussions.GetConflictingAsync(dto.StartTime, dto.EndTime, dto.Place, dto.CommitteeId, dto.TeamId, cancellationToken);
        if (currentId.HasValue)
        {
            conflicts = conflicts.Where(c => c.DiscussionId != currentId.Value).ToList();
        }

        if (conflicts.Any())
        {
            return Result.Failure("موعد المناقشة يتعارض مع موعد سابق. / Discussion schedule conflicts with an existing entry.");
        }

        return Result.Success();
    }
}
