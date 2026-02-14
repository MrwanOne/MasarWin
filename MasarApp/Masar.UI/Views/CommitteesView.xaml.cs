using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class CommitteesView : UserControl
{
    public CommitteesView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CommitteesViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
