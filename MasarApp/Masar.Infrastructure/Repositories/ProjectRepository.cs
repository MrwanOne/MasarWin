using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class ProjectRepository : EfRepository<Project>, IProjectRepository
{
    public ProjectRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<Project?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        var normalized = title.Trim().ToLower();
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Projects.FirstOrDefaultAsync(p => p.Title.ToLower() == normalized, cancellationToken);
    }

    public async Task<Project?> GetByTeamIdAsync(int teamId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Projects.FirstOrDefaultAsync(p => p.TeamId == teamId, cancellationToken);
    }

    public async Task<List<Project>> GetWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Projects.AsNoTracking()
            .Include(p => p.Department)
            .ThenInclude(d => d!.College)
            .Include(p => p.Team)
            .Include(p => p.Supervisor)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Projects
            .Include(p => p.Department)
            .ThenInclude(d => d!.College)
            .Include(p => p.Team)
            .Include(p => p.Supervisor)
            .FirstOrDefaultAsync(p => p.ProjectId == id, cancellationToken);
    }
}
