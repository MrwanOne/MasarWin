using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface ITeamService
{
    Task<List<TeamDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TeamDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<TeamDto>> AddAsync(TeamDto dto, CancellationToken cancellationToken = default);
    Task<Result<TeamDto>> UpdateAsync(TeamDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<TeamDto>> AssignSupervisorAsync(int teamId, int? supervisorId, CancellationToken cancellationToken = default);
    Task<Result<TeamDto>> AssignCommitteeAsync(int teamId, int? committeeId, CancellationToken cancellationToken = default);
}
