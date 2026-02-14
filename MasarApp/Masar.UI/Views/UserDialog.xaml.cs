using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class UserDialog : Window
{
    public UserDialog(UserEditViewModel viewModel)
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
