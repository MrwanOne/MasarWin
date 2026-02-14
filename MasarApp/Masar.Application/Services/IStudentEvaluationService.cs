using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IStudentEvaluationService
{
    Task<List<StudentEvaluationDto>> GetByDiscussionAsync(int discussionId, CancellationToken cancellationToken = default);
    Task<StudentEvaluationDto?> GetByIdAsync(int evaluationId, CancellationToken cancellationToken = default);
    Task<Result<StudentEvaluationDto>> AddAsync(StudentEvaluationDto dto, CancellationToken cancellationToken = default);
    Task<Result<StudentEvaluationDto>> UpdateAsync(StudentEvaluationDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int evaluationId, CancellationToken cancellationToken = default);
    Task<List<EvaluationCriteriaDto>> GetCriteriaAsync(int? departmentId = null, CancellationToken cancellationToken = default);
}
