using System;
using System.Windows;
using DineFlow.WPFApp.ViewModels;

namespace DineFlow.WPFApp.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Subscribe to ViewModel event to handle pure view-routing actions
        viewModel.LoginSuccess += ViewModel_LoginSuccess;
    }

    private void ViewModel_LoginSuccess()
    {
        // Unsubscribe to avoid memory leaks
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.LoginSuccess -= ViewModel_LoginSuccess;
        }

        // Return success and let App.xaml.cs take over window management
        DialogResult = true;
        Close();
    }

    private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // Secure forwarding pattern for WPF PasswordBox.
        // It reads the control value and updates the ViewModel property, preventing the ViewModel from knowing about UI controls.
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Password = txtPassword.Password;
        }
    }
}
