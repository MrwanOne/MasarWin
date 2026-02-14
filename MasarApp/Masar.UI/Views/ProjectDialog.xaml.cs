using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class ProjectDialog : Window
{
    public ProjectDialog(ProjectEditViewModel viewModel)
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
