using Masar.UI.Controls;
using Masar.UI.Services;
using System.Windows.Documents;

namespace Masar.UI.ViewModels;

public class ReportPreviewViewModel : DialogViewModel
{
    private readonly FlowDocument _document;
    private readonly ILocalizationService _localizationService;
    public FlowDocument Document => _document;

    public RelayCommand PrintCommand { get; }
    public RelayCommand CloseCommand { get; }
    public RelayCommand ExportCommand { get; }

    public ReportPreviewViewModel(FlowDocument document, ILocalizationService localizationService)
    {
        _document = document;
        _localizationService = localizationService;
        PrintCommand = new RelayCommand(_ => Print());
        CloseCommand = new RelayCommand(_ => Close(false));
        ExportCommand = new RelayCommand(_ => Close(true));
    }

    private void Print()
    {
        var dialog = new System.Windows.Controls.PrintDialog();
        if (dialog.ShowDialog() == true)
        {
            dialog.PrintDocument(((IDocumentPaginatorSource)_document).DocumentPaginator, _localizationService.GetString("Report.DocumentTitle"));
        }
    }
}
