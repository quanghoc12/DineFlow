using DineFlow.BusinessObjects.Auth;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Views;

public partial class UserEditorWindow : Window
{
    private readonly UserSummary? _user;

    public UserEditorWindow(UserSummary? user = null)
    {
        InitializeComponent();
        _user = user;

        if (user is null)
        {
            return;
        }

        TitleText.Text = "Chỉnh sửa người dùng";
        UsernameTextBox.Text = user.Username;
        FullNameTextBox.Text = user.FullName;
        RoleComboBox.SelectedIndex = user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
        PasswordPanel.Visibility = Visibility.Collapsed;
    }

    public CreateUserRequest CreateRequest => new()
    {
        Username = UsernameTextBox.Text,
        FullName = FullNameTextBox.Text,
        Role = SelectedRole,
        Password = PasswordBox.Password
    };

    public UpdateUserRequest UpdateRequest => new()
    {
        UserId = _user!.UserId,
        Username = UsernameTextBox.Text,
        FullName = FullNameTextBox.Text,
        Role = SelectedRole
    };

    private string SelectedRole =>
        (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Staff";

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) ||
            string.IsNullOrWhiteSpace(FullNameTextBox.Text) ||
            (_user is null && PasswordBox.Password.Length < 6))
        {
            ErrorText.Text = "Điền đầy đủ thông tin; mật khẩu phải có ít nhất 6 ký tự.";
            return;
        }

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
