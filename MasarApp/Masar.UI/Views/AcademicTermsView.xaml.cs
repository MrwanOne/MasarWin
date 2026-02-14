using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class AcademicTermsView : UserControl
{
    public AcademicTermsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is AcademicTermsViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
