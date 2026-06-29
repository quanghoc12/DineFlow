using System.Windows;
using DineFlow.WPFApp.ViewModels;

namespace DineFlow.WPFApp.Views;

public partial class UserEditorWindow : Window
{
    public UserEditorWindow(UserEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += ViewModel_RequestClose;
    }

    private void ViewModel_RequestClose()
    {
        if (DataContext is UserEditorViewModel viewModel)
        {
            viewModel.RequestClose -= ViewModel_RequestClose;
        }
        DialogResult = true;
        Close();
    }

    private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserEditorViewModel viewModel)
        {
            viewModel.Password = txtPassword.Password;
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
