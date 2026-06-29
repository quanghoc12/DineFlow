using System.Windows;

namespace DineFlow.WPFApp.Views;

public partial class ResetPasswordWindow : Window
{
    private readonly bool _requireCurrentPassword;

    public ResetPasswordWindow(string username, bool requireCurrentPassword = true)
    {
        InitializeComponent();
        _requireCurrentPassword = requireCurrentPassword;
        DescriptionText.Text = $"Đặt lại mật khẩu cho {username}";
        CurrentPasswordPanel.Visibility = requireCurrentPassword
            ? Visibility.Visible
            : Visibility.Collapsed;
        OwnerPermissionNotice.Visibility = requireCurrentPassword
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public string NewPassword => NewPasswordBox.Password;
    public string CurrentPassword => CurrentPasswordBox.Password;

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_requireCurrentPassword && string.IsNullOrEmpty(CurrentPassword))
        {
            ErrorText.Text = "Vui lòng nhập mật khẩu cũ.";
            return;
        }

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
