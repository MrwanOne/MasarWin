using Masar.Application.DTOs;
using Masar.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Masar.Application.Reporting.Components;

/// <summary>
/// مكون الإحصائيات والملخصات
/// </summary>
public class StatisticsComponent : IComponent
{
    private readonly ReportResultDto _reportData;
    private readonly bool _isArabic;

    public StatisticsComponent(ReportResultDto reportData, bool isArabic)
    {
        _reportData = reportData;
        _isArabic = isArabic;
    }

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(15);

            // عنوان القسم
            column.Item().Text(_isArabic ? "الإحصائيات والملخص" : "Statistics and Summary")
                .FontSize(16)
                .Bold()
                .FontColor(Colors.Blue.Darken3);

            // بطاقات الإحصائيات
            column.Item().Row(row =>
            {
                row.Spacing(10);

                // إجمالي المشاريع
                row.RelativeItem().Element(c => StatCard(c, 
                    _isArabic ? "إجمالي المشاريع" : "Total Projects",
                    _reportData.Projects.Count.ToString(),
                    Colors.Blue.Lighten3));

                // المشاريع المكتملة
                var completed = _reportData.Projects.Count(p => p.Status == ProjectStatus.Completed);
                row.RelativeItem().Element(c => StatCard(c,
                    _isArabic ? "المكتملة" : "Completed",
                    completed.ToString(),
                    Colors.Green.Lighten3));

                // قيد التنفيذ
                var inProgress = _reportData.Projects.Count(p => p.Status == ProjectStatus.InProgress);
                row.RelativeItem().Element(c => StatCard(c,
                    _isArabic ? "قيد التنفيذ" : "In Progress",
                    inProgress.ToString(),
                    Colors.Orange.Lighten3));

                // المعتمدة
                var approved = _reportData.Projects.Count(p => p.Status == ProjectStatus.Approved);
                row.RelativeItem().Element(c => StatCard(c,
                    _isArabic ? "المعتمدة" : "Approved",
                    approved.ToString(),
                    Colors.Cyan.Lighten3));
            });

            // جدول توزيع الحالات
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                // رأس الجدول
                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text(_isArabic ? "الحالة" : "Status").Bold();
                    header.Cell().Element(HeaderStyle).Text(_isArabic ? "العدد" : "Count").Bold();
                    header.Cell().Element(HeaderStyle).Text(_isArabic ? "النسبة" : "Percentage").Bold();
                });

                // البيانات
                var statusGroups = _reportData.Projects
                    .GroupBy(p => p.Status)
                    .OrderByDescending(g => g.Count());

                foreach (var group in statusGroups)
                {
                    var count = group.Count();
                    var percentage = _reportData.Projects.Count > 0 
                        ? (count * 100.0 / _reportData.Projects.Count) 
                        : 0;

                    table.Cell().Element(DataStyle).Text(ReportHelpers.GetStatusText(group.Key, _isArabic));
                    table.Cell().Element(DataStyle).Text(count.ToString()).AlignCenter();
                    table.Cell().Element(DataStyle).Text($"{percentage:0.0}%").AlignCenter();
                }

                static IContainer HeaderStyle(IContainer c) => 
                    c.Background(Colors.Grey.Lighten3).Padding(8).AlignCenter();

                static IContainer DataStyle(IContainer c) => 
                    c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8);
            });

            // متوسط نسبة الإنجاز
            if (_reportData.Projects.Any())
            {
                var avgCompletion = _reportData.Projects.Average(p => p.CompletionRate);
                column.Item().PaddingTop(10)
                    .Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(12));
                        text.Span(_isArabic ? "متوسط نسبة الإنجاز: " : "Average Completion Rate: ");
                        text.Span($"{avgCompletion:0.0}%").Bold().FontColor(Colors.Green.Darken2);
                    });
            }
        });
    }

    private void StatCard(IContainer container, string label, string value, string color)
    {
        container
            .Background(color)
            .Padding(15)
            .Column(column =>
            {
                column.Item().Text(label)
                    .FontSize(11)
                    .FontColor(Colors.Grey.Darken2);

                column.Item().PaddingTop(5).Text(value)
                    .FontSize(24)
                    .Bold()
                    .FontColor(Colors.Grey.Darken4);
            });
    }

}
