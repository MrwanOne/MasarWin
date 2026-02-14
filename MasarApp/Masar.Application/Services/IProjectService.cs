using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProjectDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> AddAsync(ProjectDto dto, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> UpdateAsync(ProjectDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> AcceptAsync(int id, int? supervisorId, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> RejectAsync(int id, string reason, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> AssignSupervisorAsync(int id, int supervisorId, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> SetCompletionRateAsync(int id, decimal completionRate, CancellationToken cancellationToken = default);
}

