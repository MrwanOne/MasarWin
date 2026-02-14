using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class StudentEvaluationRepository : EfRepository<StudentEvaluation>, IStudentEvaluationRepository
{
    public StudentEvaluationRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<List<StudentEvaluation>> GetByDiscussionAsync(int discussionId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.StudentEvaluations
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.CriteriaScores)
            .ThenInclude(cs => cs.Criteria)
            .Where(e => e.DiscussionId == discussionId)
            .OrderBy(e => e.Student!.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentEvaluation?> GetWithScoresAsync(int evaluationId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.StudentEvaluations
            .Include(e => e.Student)
            .Include(e => e.CriteriaScores)
            .ThenInclude(cs => cs.Criteria)
            .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int discussionId, int studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.StudentEvaluations
            .AnyAsync(e => e.DiscussionId == discussionId && e.StudentId == studentId, cancellationToken);
    }
}

public class EvaluationCriteriaRepository : EfRepository<EvaluationCriteria>, IEvaluationCriteriaRepository
{
    public EvaluationCriteriaRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<List<EvaluationCriteria>> GetActiveAsync(int? departmentId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        var query = context.EvaluationCriteria
            .AsNoTracking()
            .Where(c => c.IsActive);

        if (departmentId.HasValue)
        {
            query = query.Where(c => c.DepartmentId == null || c.DepartmentId == departmentId);
        }
        else
        {
            query = query.Where(c => c.DepartmentId == null);
        }

        return await query.OrderBy(c => c.DisplayOrder).ToListAsync(cancellationToken);
    }
}
