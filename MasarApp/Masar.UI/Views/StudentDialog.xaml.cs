using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class StudentDialog : Window
{
    public StudentDialog(StudentEditViewModel viewModel)
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
