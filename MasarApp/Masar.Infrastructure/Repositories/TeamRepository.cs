using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class TeamRepository : EfRepository<Team>, ITeamRepository
{
    public TeamRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<List<Team>> GetWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Teams.AsNoTracking()
            .Include(t => t.Department)
            .ThenInclude(d => d!.College)
            .Include(t => t.Supervisor)
            .Include(t => t.Committee)
            .Include(t => t.Students)
            .Include(t => t.Projects)
            .ToListAsync(cancellationToken);
    }

    public async Task<Team?> GetWithDetailsAsync(int teamId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Teams
            .Include(t => t.Department)
            .ThenInclude(d => d!.College)
            .Include(t => t.Supervisor)
            .Include(t => t.Committee)
            .Include(t => t.Students)
            .Include(t => t.Projects)
            .FirstOrDefaultAsync(t => t.TeamId == teamId, cancellationToken);
    }

    public override async Task<Team?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetWithDetailsAsync(id, cancellationToken);
    }
}
