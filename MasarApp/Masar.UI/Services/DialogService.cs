using Microsoft.Win32;
using System.Windows;

namespace Masar.UI.Services;

public class DialogService : IDialogService
{
    private readonly ILocalizationService _localizationService;

    public DialogService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public void ShowMessage(string message, string title = "Masar")
    {
        MessageBox.Show(message, ResolveTitle(title), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "Masar")
    {
        MessageBox.Show(message, ResolveTitle(title), MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public bool Confirm(string message, string title = "Masar")
    {
        return MessageBox.Show(message, ResolveTitle(title), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    public string? OpenFile(string filter)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveFile(string filter)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public bool? ShowDialog(Window window)
    {
        // Safe owner assignment to avoid WPF ownership errors
        if (window.Owner == null)
        {
            // Improved window resolution
            var activeWindow = System.Windows.Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsVisible && w != window);

            if (activeWindow != null)
            {
                try
                {
                    window.Owner = activeWindow;
                }
                catch
                {
                    // Fail silently to prevent crash
                }
            }
        }
        return window.ShowDialog();
    }

    private string ResolveTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title) || title == "Masar")
        {
            return _localizationService.GetString("App.Name");
        }

        return title;
    }
}
