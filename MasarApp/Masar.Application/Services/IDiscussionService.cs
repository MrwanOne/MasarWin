using Masar.Application.Common;
using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IDiscussionService
{
    Task<List<DiscussionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DiscussionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<DiscussionDto>> ScheduleAsync(DiscussionDto dto, CancellationToken cancellationToken = default);
    Task<Result<DiscussionDto>> UpdateAsync(DiscussionDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<DiscussionDto>> SaveEvaluationAsync(int discussionId, decimal supervisorScore, decimal committeeScore, string reportText, CancellationToken cancellationToken = default);
}
