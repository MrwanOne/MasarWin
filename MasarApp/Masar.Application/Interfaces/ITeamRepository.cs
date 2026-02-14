using Masar.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

public interface ITeamRepository : IRepository<Team>
{
    Task<List<Team>> GetWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<Team?> GetWithDetailsAsync(int teamId, CancellationToken cancellationToken = default);
}
