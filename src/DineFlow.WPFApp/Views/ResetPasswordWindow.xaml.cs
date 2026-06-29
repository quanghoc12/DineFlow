using System.Windows;

namespace DineFlow.WPFApp.Views;

public partial class ResetPasswordWindow : Window
{
    public ResetPasswordWindow(string username)
    {
        InitializeComponent();
        DescriptionText.Text = $"Đặt lại mật khẩu cho {username}";
    }

    public string NewPassword => NewPasswordBox.Password;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (NewPassword.Length < 6)
        {
            ErrorText.Text = "Mật khẩu phải có ít nhất 6 ký tự.";
            return;
        }

        if (NewPassword != ConfirmPasswordBox.Password)
        {
            ErrorText.Text = "Mật khẩu xác nhận không khớp.";
            return;
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
