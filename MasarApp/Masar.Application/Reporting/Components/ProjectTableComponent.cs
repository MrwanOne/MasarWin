using Masar.Application.DTOs;
using Masar.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Masar.Application.Reporting.Components;

/// <summary>
/// مكون جدول المشاريع
/// </summary>
public class ProjectTableComponent : IComponent
{
    private readonly ReportResultDto _reportData;
    private readonly bool _isArabic;

    public ProjectTableComponent(ReportResultDto reportData, bool isArabic)
    {
        _reportData = reportData;
        _isArabic = isArabic;
    }

    public void Compose(IContainer container)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            // عنوان القسم
            column.Item().Text(_isArabic ? "قائمة المشاريع" : "Projects List")
                .FontSize(16)
                .Bold()
                .FontColor(Colors.Blue.Darken3);

            // الجدول
            column.Item().Table(table =>
            {
                // تعريف الأعمدة
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);   // #
                    columns.RelativeColumn(3);    // العنوان
                    columns.RelativeColumn(2);    // الفريق
                    columns.RelativeColumn(2.5f); // المشرف
                    columns.RelativeColumn(2.5f); // القسم
                    columns.ConstantColumn(50);   // السنة
                    columns.RelativeColumn(2);    // الحالة
                    columns.ConstantColumn(55);   // الإنجاز
                });

                // رأس الجدول
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#").Bold();
                    header.Cell().Element(CellStyle).Text(_isArabic ? "عنوان المشروع" : "Project Title").Bold();
                    header.Cell().Element(CellStyle).Text(_isArabic ? "الفريق" : "Team").Bold();
                    header.Cell().Element(CellStyle).Text(_isArabic ? "المشرف" : "Supervisor").Bold();
                    header.Cell().Element(CellStyle).Text(_isArabic ? "القسم" : "Department").Bold();
                    header.Cell().Element(CellStyle).Text(_isArabic ? "السنة" : "Year").Bold();
                    header.Cell().Element(CellStyle).Text(_isArabic ? "الحالة" : "Status").Bold();
                    header.Cell().Element(CellStyle).Text(_isArabic ? "الإنجاز" : "Progress").Bold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container
                            .Background(Colors.Blue.Lighten4)
                            .Padding(8)
                            .AlignCenter()
                            .AlignMiddle();
                    }
                });

                // صفوف البيانات
                int index = 1;
                foreach (var project in _reportData.Projects)
                {
                    var isEven = index % 2 == 0;
                    
                    table.Cell().Element(c => DataCellStyle(c, isEven)).Text(index.ToString()).AlignCenter();
                    table.Cell().Element(c => DataCellStyle(c, isEven)).Text(project.Title);
                    table.Cell().Element(c => DataCellStyle(c, isEven)).Text(project.TeamName);
                    table.Cell().Element(c => DataCellStyle(c, isEven)).Text(project.SupervisorName ?? "-");
                    table.Cell().Element(c => DataCellStyle(c, isEven)).Text(project.DepartmentName);
                    table.Cell().Element(c => DataCellStyle(c, isEven)).Text(project.ProposedAt.Year.ToString()).AlignCenter();
                    table.Cell().Element(c => DataCellStyle(c, isEven)).Text(ReportHelpers.GetStatusText(project.Status, _isArabic)).AlignCenter();
                    table.Cell().Element(c => DataCellStyle(c, isEven)).Text($"{project.CompletionRate:0}%").AlignCenter();

                    index++;
                }

                static IContainer DataCellStyle(IContainer container, bool isEven)
                {
                    return container
                        .Background(isEven ? Colors.Grey.Lighten5 : Colors.White)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(8)
                        .AlignMiddle();
                }
            });

            // إحصائية سريعة
            column.Item().PaddingTop(10)
                .Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Grey.Darken2));
                    text.Span(_isArabic ? "إجمالي المشاريع: " : "Total Projects: ");
                    text.Span(_reportData.Projects.Count.ToString()).Bold();
                });
        });
    }

}
