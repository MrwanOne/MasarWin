using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class DiscussionDialog : Window
{
    public DiscussionDialog(DiscussionEditViewModel viewModel)
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
