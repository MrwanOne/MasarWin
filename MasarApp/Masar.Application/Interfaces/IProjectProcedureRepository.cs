using System.Threading;
using System.Threading.Tasks;

namespace Masar.Application.Interfaces;

/// <summary>
/// واجهة استدعاء Stored Procedures المتعلقة بالمشاريع والمناقشات.
/// تُنفَّذ في Infrastructure وتُحقن في ProjectService وDiscussionService.
/// </summary>
public interface IProjectProcedureRepository
{
    /// <summary>
    /// تستدعي SP_ACCEPT_PROJECT:
    /// تقبل مشروعاً، تعين مشرفاً، وتسجل السجل في ProjectStatusHistory.
    /// </summary>
    /// <param name="projectId">معرف المشروع المراد قبوله</param>
    /// <param name="supervisorId">معرف المشرف المراد تعيينه (0 إذا لم يُحدد)</param>
    /// <param name="changedByUserId">معرف المستخدم الذي أجرى العملية</param>
    /// <returns>(Code=0 نجاح, Message) أو (Code=1 فشل, Message رسالة الخطأ)</returns>
    Task<(int Code, string Message)> AcceptProjectAsync(
        int projectId,
        int supervisorId,
        int changedByUserId,
        CancellationToken ct = default);

    /// <summary>
    /// تستدعي SP_SAVE_EVALUATION:
    /// تحفظ تقييم المناقشة، تحسب الدرجة النهائية، وتُحدّث المشروع إلى "منتهي".
    /// تستبدل منطق SaveEvaluationAsync في DiscussionService.
    /// </summary>
    /// <param name="discussionId">معرف المناقشة</param>
    /// <param name="supervisorScore">درجة المشرف (0–100)</param>
    /// <param name="committeeScore">درجة اللجنة (0–100)</param>
    /// <param name="reportText">نص التقرير</param>
    /// <returns>(Code=0 نجاح, Message) أو (Code=1 فشل, Message رسالة الخطأ)</returns>
    Task<(int Code, string Message)> SaveEvaluationAsync(
        int discussionId,
        decimal supervisorScore,
        decimal committeeScore,
        string reportText,
        CancellationToken ct = default);

    /// <summary>
    /// تستدعي FN_GET_SUPERVISOR_PROJECT_COUNT مباشرة من Oracle.
    /// تستبدل منطق EnsureSupervisionCapacity الذي كان يجلب كل المشاريع إلى الذاكرة.
    /// </summary>
    /// <param name="supervisorId">معرف المشرف</param>
    /// <returns>عدد المشاريع النشطة الحالية للمشرف</returns>
    Task<int> GetSupervisorActiveProjectCountAsync(
        int supervisorId,
        CancellationToken ct = default);
}
