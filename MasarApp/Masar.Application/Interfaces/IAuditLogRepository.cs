using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IAuditLogRepository
{
    Task<List<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<AuditLog>> GetByEntityAsync(string entityName, string entityId, CancellationToken cancellationToken = default);
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
