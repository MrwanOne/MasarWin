using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class CollegeDialog : Window
{
    public CollegeDialog(CollegeEditViewModel viewModel)
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
