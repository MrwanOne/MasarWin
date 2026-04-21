using Masar.Application.DTOs;
using Masar.Application.Reporting;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Masar.UI.Services;

public class ReportDocumentBuilder
{
    private readonly ILocalizationService _localizationService;

    public ReportDocumentBuilder(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public FlowDocument BuildProjectReport(ReportResultDto result)
    {
        var isArabic = _localizationService.IsArabic;
        
        var document = new FlowDocument
        {
            PageWidth = 1050,
            ColumnWidth = double.PositiveInfinity,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12,
            PagePadding = new Thickness(16)
        };
        if (System.Windows.Application.Current.Resources["AppFlowDirection"] is FlowDirection flowDirection)
        {
            document.FlowDirection = flowDirection;
        }

        // عنوان التقرير
        var title = new Paragraph(new Run(result.Title))
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
            Margin = new Thickness(0, 0, 0, 12)
        };
        document.Blocks.Add(title);

        // التحقق من وجود بيانات
        if (result.Projects == null || result.Projects.Count == 0)
        {
            var emptyMsg = new Paragraph(new Run(
                _localizationService.GetString("Message.NoProjects")))
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };
            document.Blocks.Add(emptyMsg);
            return document;
        }

        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1)
        };

        table.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // العنوان
        table.Columns.Add(new TableColumn { Width = new GridLength(1.2, GridUnitType.Star) }); // الفريق
        table.Columns.Add(new TableColumn { Width = new GridLength(1.5, GridUnitType.Star) }); // المشرف
        table.Columns.Add(new TableColumn { Width = new GridLength(1.5, GridUnitType.Star) }); // القسم
        table.Columns.Add(new TableColumn { Width = new GridLength(0.7, GridUnitType.Star) }); // السنة
        table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // الحالة
        table.Columns.Add(new TableColumn { Width = new GridLength(0.9, GridUnitType.Star) }); // الإنجاز

        var headerGroup = new TableRowGroup();
        var headerRow = new TableRow();
        headerRow.Cells.Add(CreateHeaderCell(isArabic ? "عنوان المشروع" : "Project Title"));
        headerRow.Cells.Add(CreateHeaderCell(isArabic ? "الفريق" : "Team"));
        headerRow.Cells.Add(CreateHeaderCell(isArabic ? "المشرف" : "Supervisor"));
        headerRow.Cells.Add(CreateHeaderCell(isArabic ? "القسم" : "Department"));
        headerRow.Cells.Add(CreateHeaderCell(isArabic ? "السنة" : "Year"));
        headerRow.Cells.Add(CreateHeaderCell(isArabic ? "الحالة" : "Status"));
        headerRow.Cells.Add(CreateHeaderCell(isArabic ? "الإنجاز" : "Progress"));
        headerGroup.Rows.Add(headerRow);
        table.RowGroups.Add(headerGroup);

        var bodyGroup = new TableRowGroup();
        int index = 0;
        foreach (var project in result.Projects)
        {
            var isEven = index % 2 == 0;
            var row = new TableRow();
            if (isEven)
                row.Background = new SolidColorBrush(Color.FromRgb(249, 250, 251));
            
            row.Cells.Add(CreateCell(project.Title ?? "-"));
            row.Cells.Add(CreateCell(project.TeamName ?? "-"));
            row.Cells.Add(CreateCell(project.SupervisorName ?? "-"));
            row.Cells.Add(CreateCell(project.DepartmentName ?? "-"));
            row.Cells.Add(CreateCell(project.ProposedAt.Year.ToString()));
            row.Cells.Add(CreateCell(ReportHelpers.GetStatusText(project.Status, isArabic)));
            row.Cells.Add(CreateCell($"{project.CompletionRate:0}%"));
            bodyGroup.Rows.Add(row);
            index++;
        }

        table.RowGroups.Add(bodyGroup);
        document.Blocks.Add(table);

        // إحصائية سريعة
        var summary = new Paragraph();
        summary.Margin = new Thickness(0, 12, 0, 0);
        summary.FontSize = 13;
        summary.Foreground = new SolidColorBrush(Color.FromRgb(75, 85, 99));
        summary.Inlines.Add(new Run(isArabic ? "إجمالي المشاريع: " : "Total Projects: "));
        summary.Inlines.Add(new Bold(new Run(result.Projects.Count.ToString())));
        document.Blocks.Add(summary);

        return document;
    }

    private static TableCell CreateHeaderCell(string text)
    {
        return new TableCell(new Paragraph(new Run(text))
        {
            FontWeight = FontWeights.SemiBold,
        })
        {
            Background = new SolidColorBrush(Color.FromRgb(219, 234, 254)),
            Padding = new Thickness(8, 6, 8, 6)
        };
    }

    private static TableCell CreateCell(string? text)
    {
        return new TableCell(new Paragraph(new Run(text ?? "-")))
        {
            Padding = new Thickness(8, 6, 8, 6),
            BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
            BorderThickness = new Thickness(0, 0, 0, 1)
        };
    }
}
