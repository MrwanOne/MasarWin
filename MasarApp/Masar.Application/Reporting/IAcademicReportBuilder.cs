using Masar.Application.DTOs;

namespace Masar.Application.Reporting;

/// <summary>
/// واجهة بناء التقارير الأكاديمية
/// </summary>
public interface IAcademicReportBuilder
{
    /// <summary>
    /// بناء تقرير المشاريع وحفظه كملف PDF
    /// </summary>
    /// <param name="reportData">بيانات التقرير</param>
    /// <param name="outputPath">مسار حفظ الملف</param>
    /// <param name="isArabic">true للعربية، false للإنجليزية</param>
    void GenerateProjectReport(ReportResultDto reportData, string outputPath, bool isArabic = true);

    /// <summary>
    /// بناء تقرير المشاريع وإرجاعه كـ byte array
    /// </summary>
    /// <param name="reportData">بيانات التقرير</param>
    /// <param name="isArabic">true للعربية، false للإنجليزية</param>
    /// <returns>محتوى PDF كـ byte array</returns>
    byte[] GenerateProjectReportBytes(ReportResultDto reportData, bool isArabic = true);
}
