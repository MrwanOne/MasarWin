using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class CommitteeDialog : Window
{
    public CommitteeDialog(CommitteeEditViewModel viewModel)
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
