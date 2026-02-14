using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class DiscussionsView : UserControl
{
    public DiscussionsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DiscussionsViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
