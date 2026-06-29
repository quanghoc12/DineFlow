using System.Windows;
using DineFlow.WPFApp.ViewModels;

namespace DineFlow.WPFApp.Views;

public partial class ResetPasswordWindow : Window
{
    public ResetPasswordWindow(ResetPasswordViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += ViewModel_RequestClose;
    }

    private void ViewModel_RequestClose()
    {
        if (DataContext is ResetPasswordViewModel viewModel)
        {
            viewModel.RequestClose -= ViewModel_RequestClose;
        }
        DialogResult = true;
        Close();
    }

    private void txtNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ResetPasswordViewModel viewModel)
        {
            viewModel.NewPassword = txtNewPassword.Password;
        }
    }

    private void txtConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ResetPasswordViewModel viewModel)
        {
            viewModel.ConfirmPassword = txtConfirmPassword.Password;
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
