using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class CollegesView : UserControl
{
    public CollegesView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CollegesViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
