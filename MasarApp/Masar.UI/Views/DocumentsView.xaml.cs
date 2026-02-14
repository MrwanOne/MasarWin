using System.Windows;
using System.Windows.Controls;
using Masar.UI.ViewModels;

namespace Masar.UI.Views;

public partial class DocumentsView : UserControl
{
    public DocumentsView()
    {
        InitializeComponent();
    }

    private void UserControl_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0 && DataContext is DocumentsViewModel vm)
            {
                foreach (var file in files)
                {
                    _ = vm.UploadFileInternalAsync(file);
                }
            }
        }
    }
}
