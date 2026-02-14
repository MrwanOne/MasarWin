using Masar.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Masar.UI.Views;

public partial class EvaluationsView : UserControl
{
    public EvaluationsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is EvaluationsViewModel vm)
        {
            _ = vm.LoadAsync();
        }
    }
}
