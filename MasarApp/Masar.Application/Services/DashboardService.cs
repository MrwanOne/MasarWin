using Masar.Application.DTOs;
using Masar.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Services;

/// <summary>
/// يستخدم VW_DASHBOARD_STATS لجلب الإحصائيات مباشرةً من قاعدة البيانات
/// بدلاً من جلب كل السجلات وعدّها في memory
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IViewRepository _viewRepository;

    public DashboardService(IViewRepository viewRepository)
    {
        _viewRepository = viewRepository;
    }

    public Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
        => _viewRepository.GetDashboardStatsAsync(cancellationToken);
}

