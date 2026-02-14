using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IStudentEvaluationRepository : IRepository<StudentEvaluation>
{
    Task<List<StudentEvaluation>> GetByDiscussionAsync(int discussionId, CancellationToken cancellationToken = default);
    Task<StudentEvaluation?> GetWithScoresAsync(int evaluationId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int discussionId, int studentId, CancellationToken cancellationToken = default);
}

public interface IEvaluationCriteriaRepository : IRepository<EvaluationCriteria>
{
    Task<List<EvaluationCriteria>> GetActiveAsync(int? departmentId = null, CancellationToken cancellationToken = default);
}
