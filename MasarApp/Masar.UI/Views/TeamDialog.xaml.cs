using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class TeamDialog : Window
{
    public TeamDialog(TeamEditViewModel viewModel)
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
