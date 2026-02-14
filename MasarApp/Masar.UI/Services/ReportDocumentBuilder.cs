using Masar.Application.DTOs;
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
        var document = new FlowDocument
        {
            PageWidth = 1200,
            ColumnWidth = 1200,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 12,
            PagePadding = new Thickness(40)
        };
        if (System.Windows.Application.Current.Resources["AppFlowDirection"] is FlowDirection flowDirection)
        {
            document.FlowDirection = flowDirection;
        }

        var title = new Paragraph(new Run(result.Title))
        {
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
            Margin = new Thickness(0, 0, 0, 12)
        };
        document.Blocks.Add(title);

        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
            BorderThickness = new Thickness(1)
        };

        table.Columns.Add(new TableColumn { Width = new GridLength(200) }); // Title
        table.Columns.Add(new TableColumn { Width = new GridLength(140) }); // Team
        table.Columns.Add(new TableColumn { Width = new GridLength(140) }); // Beneficiary
        table.Columns.Add(new TableColumn { Width = new GridLength(140) }); // Supervisor
        table.Columns.Add(new TableColumn { Width = new GridLength(140) }); // Department
        table.Columns.Add(new TableColumn { Width = new GridLength(80) });  // Year
        table.Columns.Add(new TableColumn { Width = new GridLength(120) }); // Status
        table.Columns.Add(new TableColumn { Width = new GridLength(100) }); // Completion

        var headerGroup = new TableRowGroup();
        var headerRow = new TableRow();
        headerRow.Cells.Add(CreateHeaderCell(_localizationService.GetString("Report.HeaderTitle")));
        headerRow.Cells.Add(CreateHeaderCell(_localizationService.GetString("Grid.Team")));
        headerRow.Cells.Add(CreateHeaderCell(_localizationService.GetString("Grid.Beneficiary")));
        headerRow.Cells.Add(CreateHeaderCell(_localizationService.GetString("Report.HeaderSupervisor")));
        headerRow.Cells.Add(CreateHeaderCell(_localizationService.GetString("Report.HeaderDepartment")));
        headerRow.Cells.Add(CreateHeaderCell(_localizationService.GetString("Label.Year")));
        headerRow.Cells.Add(CreateHeaderCell(_localizationService.GetString("Report.HeaderStatus")));
        headerRow.Cells.Add(CreateHeaderCell(_localizationService.GetString("Report.HeaderCompletion")));
        headerGroup.Rows.Add(headerRow);
        table.RowGroups.Add(headerGroup);

        var bodyGroup = new TableRowGroup();
        foreach (var project in result.Projects)
        {
            var row = new TableRow();
            row.Cells.Add(CreateCell(project.Title));
            row.Cells.Add(CreateCell(project.TeamName));
            row.Cells.Add(CreateCell(project.Beneficiary));
            row.Cells.Add(CreateCell(project.SupervisorName));
            row.Cells.Add(CreateCell(project.DepartmentName));
            row.Cells.Add(CreateCell(project.ProposedAt.Year.ToString()));
            row.Cells.Add(CreateCell(project.Status.ToString()));
            row.Cells.Add(CreateCell($"{project.CompletionRate:0}%"));
            bodyGroup.Rows.Add(row);
        }

        table.RowGroups.Add(bodyGroup);
        document.Blocks.Add(table);

        return document;
    }

    private static TableCell CreateHeaderCell(string text)
    {
        return new TableCell(new Paragraph(new Run(text)))
        {
            Background = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
            FontWeight = FontWeights.SemiBold,
            Padding = new Thickness(6)
        };
    }

    private static TableCell CreateCell(string text)
    {
        return new TableCell(new Paragraph(new Run(text)))
        {
            Padding = new Thickness(6)
        };
    }
}
