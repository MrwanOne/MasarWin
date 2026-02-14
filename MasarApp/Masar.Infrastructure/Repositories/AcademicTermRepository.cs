using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class AcademicTermRepository : EfRepository<AcademicTerm>, IAcademicTermRepository
{
    public AcademicTermRepository(IDbContextFactory<MasarDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<AcademicTerm?> GetActiveTermAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.AcademicTerms
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IsActive, cancellationToken);
    }

    public async Task<List<AcademicTerm>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        return await context.AcademicTerms
            .AsNoTracking()
            .OrderByDescending(t => t.Year)
            .ThenByDescending(t => t.Semester)
            .ToListAsync(cancellationToken);
    }

    public async Task SetActiveTermAsync(int termId, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        
        // Get current term
        var term = await context.AcademicTerms.FindAsync([termId], cancellationToken);
        if (term != null)
        {
            // Toggle active status
            term.IsActive = !term.IsActive;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int year, int semester, int? excludeTermId = null, CancellationToken cancellationToken = default)
    {
        await using var context = await CreateContextAsync(cancellationToken);
        var query = context.AcademicTerms.Where(t => t.Year == year && t.Semester == semester);
        
        if (excludeTermId.HasValue)
        {
            query = query.Where(t => t.TermId != excludeTermId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
}
