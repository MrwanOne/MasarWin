using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class TeamsView : UserControl
{
    public TeamsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is TeamsViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
