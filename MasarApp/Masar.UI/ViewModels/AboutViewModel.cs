using Masar.UI.Controls;
using Masar.UI.Services;
using System.Reflection;

namespace Masar.UI.ViewModels;

public class AboutViewModel : DialogViewModel
{
    public string AppName { get; }
    public string Version { get; }
    public string Description { get; }
    public string Developer { get; }

    public RelayCommand CloseCommand { get; }

    public AboutViewModel(ILocalizationService localizationService)
    {
        AppName = localizationService.GetString("App.Name");
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        Description = localizationService.GetString("About.Description");
        Developer = localizationService.GetString("About.Developer");
        CloseCommand = new RelayCommand(_ => Close(false));
    }
}
