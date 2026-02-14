using Masar.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
