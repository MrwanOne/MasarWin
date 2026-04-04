using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

/// <summary>
/// استعلام الـ Views في Oracle بدلاً من Include chains الثقيلة
/// </summary>
public class ViewRepository : IViewRepository
{
    private readonly IDbContextFactory<MasarDbContext> _contextFactory;

    public ViewRepository(IDbContextFactory<MasarDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_PROJECT_FULL_DETAIL
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<ProjectFullDetailDto>> GetProjectFullDetailsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<ProjectFullDetailDto>("SELECT * FROM VW_PROJECT_FULL_DETAIL")
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_STUDENT_FULL_DETAIL
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<StudentFullDetailDto>> GetStudentFullDetailsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<StudentFullDetailDto>("SELECT * FROM VW_STUDENT_FULL_DETAIL")
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_DISCUSSION_RESULTS
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<DiscussionResultDto>> GetDiscussionResultsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<DiscussionResultDto>("SELECT * FROM VW_DISCUSSION_RESULTS")
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_COMMITTEE_COMPOSITION
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<CommitteeCompositionDto>> GetCommitteeCompositionAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<CommitteeCompositionDto>("SELECT * FROM VW_COMMITTEE_COMPOSITION")
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_DEPARTMENT_STATS
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<DepartmentStatsDto>> GetDepartmentStatsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Database
            .SqlQueryRaw<DepartmentStatsDto>("SELECT * FROM VW_DEPARTMENT_STATS")
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // VW_DASHBOARD_STATS
    // ═══════════════════════════════════════════════════════════════
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var result = await context.Database
            .SqlQueryRaw<DashboardStatsDto>("SELECT * FROM VW_DASHBOARD_STATS")
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        return result ?? new DashboardStatsDto();
    }
}
