using System.Windows;

namespace Masar.UI.Services;

public interface IDialogService
{
    void ShowMessage(string message, string title = "Masar");
    void ShowError(string message, string title = "Masar");
    bool Confirm(string message, string title = "Masar");
    string? OpenFile(string filter);
    string? SaveFile(string filter);
    bool? ShowDialog(Window window);
}
