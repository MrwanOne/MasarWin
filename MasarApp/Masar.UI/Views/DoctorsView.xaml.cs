using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class DoctorsView : UserControl
{
    public DoctorsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DoctorsViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
