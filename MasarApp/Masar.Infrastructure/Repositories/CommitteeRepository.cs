using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class CommitteeRepository : EfRepository<Committee>, ICommitteeRepository
{
    public CommitteeRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<List<Committee>> GetWithMembersAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Committees.AsNoTracking()
            .Include(c => c.Department)
            .ThenInclude(d => d!.College)
            .Include(c => c.Members)
            .ThenInclude(m => m.Doctor)
            .ToListAsync(cancellationToken);
    }

    public async Task<Committee?> GetWithMembersAsync(int committeeId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Committees
            .Include(c => c.Department)
            .ThenInclude(d => d!.College)
            .Include(c => c.Members)
            .ThenInclude(m => m.Doctor)
            .FirstOrDefaultAsync(c => c.CommitteeId == committeeId, cancellationToken);
    }

    public async Task<bool> DoctorHasCommitteeInTermAsync(int doctorId, int termId, int? excludeCommitteeId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.CommitteeMembers
            .Include(cm => cm.Committee)
            .AnyAsync(cm => cm.DoctorId == doctorId
                            && cm.Committee!.TermId == termId
                            && (!excludeCommitteeId.HasValue || cm.CommitteeId != excludeCommitteeId.Value),
                cancellationToken);
    }

    public override async Task<Committee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.Committees
            .Include(c => c.Department)
            .ThenInclude(d => d!.College)
            .Include(c => c.Members)
            .ThenInclude(m => m.Doctor)
            .FirstOrDefaultAsync(c => c.CommitteeId == id, cancellationToken);
    }

    public async Task AddMemberAsync(CommitteeMember member, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        await context.CommitteeMembers.AddAsync(member, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateMemberAsync(CommitteeMember member, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        context.CommitteeMembers.Update(member);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(int committeeId, int doctorId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        var existing = await context.CommitteeMembers
            .FirstOrDefaultAsync(m => m.CommitteeId == committeeId && m.DoctorId == doctorId, cancellationToken);
        if (existing != null)
        {
            context.CommitteeMembers.Remove(existing);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<CommitteeMember?> GetMemberAsync(int committeeId, int doctorId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.CommitteeMembers
            .FirstOrDefaultAsync(m => m.CommitteeId == committeeId && m.DoctorId == doctorId, cancellationToken);
    }
}
