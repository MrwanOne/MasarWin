using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class StudentsView : UserControl
{
    public StudentsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is StudentsViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
