using Masar.UI.Controls;
using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class ProjectsView : UserControl
{
    public ProjectsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProjectsViewModel vm)
        {
            NotifyTask.Create(vm.LoadAsync());
        }
    }
}
