using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class DepartmentsView : UserControl
{
    public DepartmentsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DepartmentsViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
