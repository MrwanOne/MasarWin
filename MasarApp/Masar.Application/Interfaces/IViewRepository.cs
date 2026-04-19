using Masar.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

/// <summary>
/// استعلامات الـ Views في Oracle
/// </summary>
public interface IViewRepository
{
    /// <summary> VW_PROJECT_FULL_DETAIL — تفاصيل المشاريع الكاملة </summary>
    Task<List<ProjectFullDetailDto>> GetProjectFullDetailsAsync(CancellationToken ct = default);

    /// <summary> VW_STUDENT_FULL_DETAIL — البيانات الشاملة للطلاب </summary>
    Task<List<StudentFullDetailDto>> GetStudentFullDetailsAsync(CancellationToken ct = default);

    /// <summary> VW_DISCUSSION_RESULTS — نتائج جلسات المناقشة </summary>
    Task<List<DiscussionResultDto>> GetDiscussionResultsAsync(CancellationToken ct = default);

    /// <summary> VW_COMMITTEE_COMPOSITION — تشكيل اللجان وأعضائها </summary>
    Task<List<CommitteeCompositionDto>> GetCommitteeCompositionAsync(CancellationToken ct = default);

    /// <summary> VW_DEPARTMENT_STATS — إحصائيات الأقسام التحليلية </summary>
    Task<List<DepartmentStatsDto>> GetDepartmentStatsAsync(CancellationToken ct = default);

    /// <summary> VW_DASHBOARD_STATS — إحصائيات لوحة التحكم </summary>
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken ct = default);
}
