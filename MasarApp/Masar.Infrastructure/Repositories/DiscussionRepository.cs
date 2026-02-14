using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class DiscussionRepository : EfRepository<Discussion>, IDiscussionRepository
{
    public DiscussionRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<List<Discussion>> GetWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Discussions.AsNoTracking()
            .Include(d => d.Team)
            .Include(d => d.Committee)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Discussion>> GetConflictingAsync(DateTime start, DateTime end, string place, int? committeeId, int? teamId, CancellationToken cancellationToken = default)
    {
        var normalizedPlace = place.Trim();
        await using var context = await CreateContextAsync(cancellationToken);
        var query = context.Discussions.AsNoTracking()
            .Where(d => d.StartTime < end && d.EndTime > start)
            .Where(d => d.Place == normalizedPlace || (committeeId.HasValue && d.CommitteeId == committeeId.Value) || (teamId.HasValue && d.TeamId == teamId.Value));

        return await query.ToListAsync(cancellationToken);
    }

    public override async Task<Discussion?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Discussions
            .Include(d => d.Team)
            .Include(d => d.Committee)
            .FirstOrDefaultAsync(d => d.DiscussionId == id, cancellationToken);
    }
}
