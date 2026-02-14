using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class CommitteeMemberDialog : Window
{
    public CommitteeMemberDialog(CommitteeMemberAssignViewModel viewModel)
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
