using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class DepartmentPickerDialog : Window
{
    public DepartmentPickerDialog(DepartmentPickerViewModel viewModel)
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
