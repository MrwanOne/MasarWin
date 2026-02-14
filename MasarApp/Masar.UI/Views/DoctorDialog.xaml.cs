using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class DoctorDialog : Window
{
    public DoctorDialog(DoctorEditViewModel viewModel)
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
