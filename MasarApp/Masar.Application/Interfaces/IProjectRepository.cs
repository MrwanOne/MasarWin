using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project?> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
    Task<Project?> GetByTeamIdAsync(int teamId, CancellationToken cancellationToken = default);
    Task<List<Project>> GetWithDetailsAsync(CancellationToken cancellationToken = default);
}
