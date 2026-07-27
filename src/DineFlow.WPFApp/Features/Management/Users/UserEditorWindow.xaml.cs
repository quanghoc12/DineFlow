using DineFlow.BusinessObjects.Auth;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Management.Users;

public partial class UserEditorWindow : Window
{
    private readonly UserSummary? _user;
    public bool DesiredIsActive { get; private set; } = true;

    public UserEditorWindow(UserSummary? user = null, string actorRole = "Admin")
    {
        InitializeComponent();
        _user = user;
        bool actorIsOwner = AuthRoles.IsOwner(actorRole);
        OwnerRoleOption.Visibility = actorIsOwner ? Visibility.Visible : Visibility.Collapsed;

        if (user is null)
        {
            RoleComboBox.SelectedIndex = 2;
            return;
        }

        TitleText.Text = "Chỉnh sửa người dùng";
        UsernameTextBox.Text = user.Username;
        FullNameTextBox.Text = user.FullName;
        RoleComboBox.SelectedItem = RoleComboBox.Items.Cast<ComboBoxItem>()
            .First(item => (item.Tag?.ToString() ?? string.Empty)
                .Equals(user.Role, StringComparison.OrdinalIgnoreCase));
        PasswordPanel.Visibility = Visibility.Collapsed;
        AccountStatusPanel.Visibility = Visibility.Visible;
        DesiredIsActive = user.IsActive;
        if (!actorIsOwner &&
            (AuthRoles.IsAdmin(user.Role) || AuthRoles.IsOwner(user.Role)))
        {
            RoleComboBox.IsEnabled = false;
            ToggleAccountButton.Visibility = Visibility.Collapsed;
        }
        RefreshAccountStatus();
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
        (RoleComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Staff";

    private void ToggleAccountButton_Click(object sender, RoutedEventArgs e)
    {
        DesiredIsActive = !DesiredIsActive;
        RefreshAccountStatus();
    }

    private void RefreshAccountStatus()
    {
        AccountStatusText.Text = DesiredIsActive ? "Tài khoản đang hoạt động." : "Tài khoản sẽ bị khóa.";
        ToggleAccountButton.Content = DesiredIsActive ? "Khóa tài khoản" : "Mở khóa tài khoản";
    }

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
