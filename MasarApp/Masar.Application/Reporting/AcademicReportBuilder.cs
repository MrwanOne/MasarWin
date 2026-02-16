using Masar.Application.DTOs;
using Masar.Application.Reporting.Components;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Masar.Application.Reporting;

/// <summary>
/// بناء التقارير الأكاديمية باستخدام QuestPDF
/// </summary>
public class AcademicReportBuilder : IAcademicReportBuilder
{
    public void GenerateProjectReport(ReportResultDto reportData, string outputPath, bool isArabic = true)
    {
        ArgumentNullException.ThrowIfNull(reportData);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        var document = CreateDocument(reportData, isArabic);
        document.GeneratePdf(outputPath);
    }

    public byte[] GenerateProjectReportBytes(ReportResultDto reportData, bool isArabic = true)
    {
        ArgumentNullException.ThrowIfNull(reportData);

        var document = CreateDocument(reportData, isArabic);
        return document.GeneratePdf();
    }

    private Document CreateDocument(ReportResultDto reportData, bool isArabic)
    {
        return Document.Create(container =>
        {
            // صفحة الغلاف
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                
                if (isArabic)
                    page.ContentFromRightToLeft();

                // خط افتراضي يدعم العربية
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI"));

                var coverPage = new CoverPageComponent(reportData, isArabic);
                page.Content().Component(coverPage);
            });

            // الصفحات الرئيسية
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                if (isArabic)
                    page.ContentFromRightToLeft();

                // خط افتراضي يدعم العربية
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                // الترويسة والتذييل
                var headerFooter = new HeaderFooterComponent(reportData.Title, isArabic);
                page.Header().Element(headerFooter.ComposeHeader);
                page.Footer().Element(headerFooter.ComposeFooter);

                // المحتوى
                page.Content().Column(column =>
                {
                    column.Spacing(20);

                    // قسم الإحصائيات
                    column.Item().Component(new StatisticsComponent(reportData, isArabic));

                    // فاصل
                    column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // جدول المشاريع
                    column.Item().Component(new ProjectTableComponent(reportData, isArabic));
                });
            });
        });
    }
}
