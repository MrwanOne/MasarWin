using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IProjectStatusHistoryRepository : IRepository<ProjectStatusHistory>
{
    Task<List<ProjectStatusHistory>> GetByProjectIdAsync(int projectId, CancellationToken cancellationToken = default);
}
