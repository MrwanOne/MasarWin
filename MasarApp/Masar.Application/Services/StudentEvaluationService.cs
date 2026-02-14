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

public class StudentEvaluationService : IStudentEvaluationService
{
    private readonly IStudentEvaluationRepository _evaluations;
    private readonly IEvaluationCriteriaRepository _criteria;
    private readonly ICurrentUserService _currentUser;

    public StudentEvaluationService(
        IStudentEvaluationRepository evaluations,
        IEvaluationCriteriaRepository criteria,
        ICurrentUserService currentUser)
    {
        _evaluations = evaluations;
        _criteria = criteria;
        _currentUser = currentUser;
    }

    public async Task<List<StudentEvaluationDto>> GetByDiscussionAsync(int discussionId, CancellationToken cancellationToken = default)
    {
        var evaluations = await _evaluations.GetByDiscussionAsync(discussionId, cancellationToken);
        return evaluations.Select(ToDto).ToList();
    }

    public async Task<StudentEvaluationDto?> GetByIdAsync(int evaluationId, CancellationToken cancellationToken = default)
    {
        var evaluation = await _evaluations.GetWithScoresAsync(evaluationId, cancellationToken);
        return evaluation == null ? null : ToDto(evaluation);
    }

    public async Task<Result<StudentEvaluationDto>> AddAsync(StudentEvaluationDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<StudentEvaluationDto>.Failure("User is not authenticated.");

        if (_currentUser.Role != UserRole.Admin && _currentUser.Role != UserRole.HeadOfDepartment && _currentUser.Role != UserRole.Supervisor)
            return Result<StudentEvaluationDto>.Failure("User is not authorized to add evaluations.");

        if (await _evaluations.ExistsAsync(dto.DiscussionId, dto.StudentId, cancellationToken))
            return Result<StudentEvaluationDto>.Failure("Student already has an evaluation for this discussion.");

        var entity = new StudentEvaluation
        {
            DiscussionId = dto.DiscussionId,
            StudentId = dto.StudentId,
            TotalScore = dto.TotalScore,
            ContributionPercentage = dto.ContributionPercentage,
            GeneralFeedback = dto.GeneralFeedback,
            StrengthPoints = dto.StrengthPoints,
            ImprovementAreas = dto.ImprovementAreas,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId,
            CriteriaScores = dto.CriteriaScores.Select(cs => new CriteriaScore
            {
                CriteriaId = cs.CriteriaId,
                Score = cs.Score,
                Comments = cs.Comments
            }).ToList()
        };

        await _evaluations.AddAsync(entity, cancellationToken);
        var created = await _evaluations.GetWithScoresAsync(entity.EvaluationId, cancellationToken);
        return Result<StudentEvaluationDto>.Success(ToDto(created ?? entity));
    }

    public async Task<Result<StudentEvaluationDto>> UpdateAsync(StudentEvaluationDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<StudentEvaluationDto>.Failure("User is not authenticated.");

        var entity = await _evaluations.GetWithScoresAsync(dto.EvaluationId, cancellationToken);
        if (entity == null)
            return Result<StudentEvaluationDto>.Failure("Evaluation not found.");

        entity.TotalScore = dto.TotalScore;
        entity.ContributionPercentage = dto.ContributionPercentage;
        entity.GeneralFeedback = dto.GeneralFeedback;
        entity.StrengthPoints = dto.StrengthPoints;
        entity.ImprovementAreas = dto.ImprovementAreas;
        entity.CreatedAt = DateTime.UtcNow; // Updated to CreatedAt

        // Update criteria scores
        entity.CriteriaScores.Clear();
        foreach (var cs in dto.CriteriaScores)
        {
            entity.CriteriaScores.Add(new CriteriaScore
            {
                CriteriaId = cs.CriteriaId,
                Score = cs.Score,
                Comments = cs.Comments
            });
        }

        await _evaluations.UpdateAsync(entity, cancellationToken);
        var updated = await _evaluations.GetWithScoresAsync(entity.EvaluationId, cancellationToken);
        return Result<StudentEvaluationDto>.Success(ToDto(updated ?? entity));
    }

    public async Task<Result> DeleteAsync(int evaluationId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
            return Result.Failure("User is not authenticated.");

        var entity = await _evaluations.GetByIdAsync(evaluationId, cancellationToken);
        if (entity == null)
            return Result.Failure("Evaluation not found.");

        await _evaluations.DeleteAsync(entity, cancellationToken);
        return Result.Success();
    }

    public async Task<List<EvaluationCriteriaDto>> GetCriteriaAsync(int? departmentId = null, CancellationToken cancellationToken = default)
    {
        var criteria = await _criteria.GetActiveAsync(departmentId, cancellationToken);
        return criteria.Select(c => new EvaluationCriteriaDto
        {
            CriteriaId = c.CriteriaId,
            NameAr = c.NameAr,
            NameEn = c.NameEn,
            DescriptionAr = c.DescriptionAr,
            DescriptionEn = c.DescriptionEn,
            MaxScore = c.MaxScore,
            Weight = c.Weight,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            DepartmentId = c.DepartmentId
        }).ToList();
    }

    private static StudentEvaluationDto ToDto(StudentEvaluation entity) => new()
    {
        EvaluationId = entity.EvaluationId,
        DiscussionId = entity.DiscussionId,
        StudentId = entity.StudentId,
        StudentName = entity.Student?.FullName ?? string.Empty,
        StudentNumber = entity.Student?.StudentNumber ?? string.Empty,
        TotalScore = entity.TotalScore,
        ContributionPercentage = entity.ContributionPercentage,
        GeneralFeedback = entity.GeneralFeedback,
        StrengthPoints = entity.StrengthPoints,
        ImprovementAreas = entity.ImprovementAreas,
        EvaluatedAt = entity.CreatedAt,
        EvaluatedByUserId = entity.CreatedByUserId,
        EvaluatedByName = string.Empty, // Navigation not available in BaseEntity
        CriteriaScores = entity.CriteriaScores.Select(cs => new CriteriaScoreDto
        {
            ScoreId = cs.ScoreId,
            EvaluationId = cs.EvaluationId,
            CriteriaId = cs.CriteriaId,
            CriteriaNameAr = cs.Criteria?.NameAr ?? string.Empty,
            CriteriaNameEn = cs.Criteria?.NameEn ?? string.Empty,
            MaxScore = cs.Criteria?.MaxScore ?? 0,
            Score = cs.Score,
            Comments = cs.Comments
        }).ToList()
    };
}
