using Masar.Application.Interfaces;
using Masar.Domain.Entities;
using Masar.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbContextFactory<MasarDbContext> _dbFactory;

    public AuditLogRepository(IDbContextFactory<MasarDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await context.AuditLogs
            .OrderByDescending(x => x.ChangedAt)
            .Take(1000)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityName, string entityId, CancellationToken cancellationToken = default)
    {
        using var context = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await context.AuditLogs
            .Where(x => x.EntityName == entityName && x.EntityId == entityId)
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        using var context = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await context.AuditLogs.AddAsync(auditLog, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
