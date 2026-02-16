using QuestPDF.Infrastructure;

namespace Masar.Application.Reporting;

/// <summary>
/// إعداد وتهيئة QuestPDF
/// </summary>
public static class QuestPdfConfiguration
{
    /// <summary>
    /// تهيئة QuestPDF - يجب استدعاؤها عند بدء التطبيق
    /// </summary>
    public static void Initialize()
    {
        // تسجيل الترخيص - مشروع أكاديمي/مفتوح المصدر
        QuestPDF.Settings.License = LicenseType.Community;
    }
}
