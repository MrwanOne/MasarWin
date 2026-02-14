using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class DepartmentDialog : Window
{
    public DepartmentDialog(DepartmentEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += (_, result) =>
        {
            DialogResult = result;
            Close();
        };
    }
}
