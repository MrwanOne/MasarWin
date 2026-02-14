using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class ProjectStatusHistoryRepository : EfRepository<ProjectStatusHistory>, IProjectStatusHistoryRepository
{
    public ProjectStatusHistoryRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<List<ProjectStatusHistory>> GetByProjectIdAsync(int projectId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.ProjectStatusHistories
            .Where(h => h.ProjectId == projectId)
            .Include(h => h.ChangedByUser)
                .ThenInclude(u => u!.Doctor)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(cancellationToken);
    }
}
