using Masar.Domain.Enums;

namespace Masar.Application.Reporting;

/// <summary>
/// أدوات مساعدة مشتركة للتقارير
/// </summary>
public static class ReportHelpers
{
    /// <summary>
    /// ترجمة حالة المشروع حسب اللغة
    /// </summary>
    public static string GetStatusText(ProjectStatus status, bool isArabic)
    {
        if (!isArabic)
            return status.ToString();

        return status switch
        {
            ProjectStatus.Proposed => "مقترح",
            ProjectStatus.Approved => "معتمد",
            ProjectStatus.InProgress => "قيد التنفيذ",
            ProjectStatus.Completed => "مكتمل",
            ProjectStatus.Rejected => "مرفوض",
            _ => status.ToString()
        };
    }
}
