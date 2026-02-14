using Masar.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace Masar.UI;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.LoginSucceeded += (_, _) =>
        {
            var main = serviceProvider.GetRequiredService<MainWindow>();
            System.Windows.Application.Current.MainWindow = main;
            main.Show();
            Close();
        };
    }
}
