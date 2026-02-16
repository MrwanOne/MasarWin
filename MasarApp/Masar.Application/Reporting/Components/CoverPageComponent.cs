using Masar.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;

namespace Masar.Application.Reporting.Components;

/// <summary>
/// مكون صفحة الغلاف الأكاديمية مع شعار الجامعة
/// </summary>
public class CoverPageComponent : IComponent
{
    private readonly ReportResultDto _reportData;
    private readonly bool _isArabic;

    public CoverPageComponent(ReportResultDto reportData, bool isArabic)
    {
        _reportData = reportData;
        _isArabic = isArabic;
    }

    public void Compose(IContainer container)
    {
        container
            .Border(2)
            .BorderColor(Colors.Blue.Darken3)
            .Padding(30)
            .Column(column =>
        {
            column.Spacing(15);

            // المساحة العلوية
            column.Item().PaddingTop(30);

            // شعار الجامعة
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.EndsWith("university_logo.png"));
                
                if (resourceName != null)
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        var logoBytes = ms.ToArray();
                        column.Item().AlignCenter().Width(120).Image(logoBytes);
                    }
                }
            }
            catch
            {
                // إذا فشل تحميل الشعار، نتابع بدونه
            }

            // خط زخرفي علوي
            column.Item().PaddingTop(10).LineHorizontal(3).LineColor(Colors.Blue.Darken3);

            // اسم الجامعة
            column.Item().AlignCenter().Text(_isArabic ? "جامعة إقليم سبأ" : "Saba Region University")
                .FontSize(26)
                .Bold()
                .FontColor(Colors.Blue.Darken3);

            // اسم الكلية
            column.Item().AlignCenter().Text(_isArabic ? "كلية الحاسب الآلي" : "College of Computer Science")
                .FontSize(18)
                .SemiBold()
                .FontColor(Colors.Blue.Darken2);

            // خط فاصل
            column.Item().PaddingVertical(15).LineHorizontal(2).LineColor(Colors.Blue.Lighten2);

            // عنوان التقرير
            column.Item().AlignCenter().Text(_reportData.Title)
                .FontSize(22)
                .Bold()
                .FontColor(Colors.Grey.Darken4);

            // نوع التقرير
            column.Item().AlignCenter().Text(_isArabic ? "تقرير مشاريع التخرج" : "Graduation Projects Report")
                .FontSize(16)
                .FontColor(Colors.Grey.Darken2);

            // عدد المشاريع
            column.Item().AlignCenter().Text(
                _isArabic 
                    ? $"عدد المشاريع: {_reportData.Projects.Count}" 
                    : $"Total Projects: {_reportData.Projects.Count}")
                .FontSize(14)
                .FontColor(Colors.Grey.Darken1);

            // المساحة السفلية
            column.Item().PaddingTop(60);

            // التاريخ
            column.Item().AlignCenter()
                .Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(14).FontColor(Colors.Grey.Darken1));
                    text.Span(_isArabic ? "تاريخ الإصدار: " : "Issue Date: ");
                    text.Span(DateTime.Now.ToString(_isArabic ? "yyyy/MM/dd" : "dd/MM/yyyy")).Bold();
                });

            // السنة الأكاديمية
            column.Item().AlignCenter().Text(
                _isArabic 
                    ? $"العام الأكاديمي {DateTime.Now.Year}" 
                    : $"Academic Year {DateTime.Now.Year}")
                .FontSize(16)
                .SemiBold()
                .FontColor(Colors.Grey.Darken2);

            // خط زخرفي سفلي
            column.Item().PaddingTop(10).LineHorizontal(3).LineColor(Colors.Blue.Darken3);
        });
    }
}
