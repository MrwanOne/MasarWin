namespace Masar.UI.ViewModels;

public class NavigationItemViewModel : ViewModelBase
{
    public string Title { get; }
    public string Icon { get; }
    public ViewModelBase ViewModel { get; }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public NavigationItemViewModel(string title, string icon, ViewModelBase viewModel)
    {
        Title = title;
        Icon = icon;
        ViewModel = viewModel;
    }
}
