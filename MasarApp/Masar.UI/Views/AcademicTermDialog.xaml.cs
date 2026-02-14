using Masar.UI.ViewModels;
using System.Windows;

namespace Masar.UI.Views;

public partial class AcademicTermDialog : Window
{
    public AcademicTermDialog(AcademicTermEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
