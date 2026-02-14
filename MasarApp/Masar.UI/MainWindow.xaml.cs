using Masar.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace Masar.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.LogoutRequested += (_, _) =>
        {
            var login = serviceProvider.GetRequiredService<LoginWindow>();
            login.Show();
            Close();
        };
    }
}
