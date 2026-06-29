using DineFlow.BusinessObjects.Auth;
using DineFlow.WPFApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Views;

public partial class UserManagementView : UserControl
{
    private readonly UserManagementViewModel _viewModel;

    public UserManagementView(UserManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public Task LoadAsync() => _viewModel.LoadAsync();

    private async void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        UserEditorWindow dialog = new();
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.CreateAsync(dialog.CreateRequest);
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out UserSummary user))
        {
            return;
        }

        UserEditorWindow dialog = new(user);
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.UpdateAsync(dialog.UpdateRequest);
        }
    }

    private async void ToggleActiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out UserSummary user))
        {
            return;
        }

        string action = user.IsActive ? "khóa" : "mở khóa";
        if (MessageBox.Show(
                $"Bạn có chắc muốn {action} tài khoản {user.Username}?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            await _viewModel.SetActiveAsync(user, !user.IsActive);
        }
    }

    private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out UserSummary user))
        {
            return;
        }

        ResetPasswordWindow dialog = new(user.Username);
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.ResetPasswordAsync(user, dialog.NewPassword);
            if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                MessageBox.Show("Đã đặt lại mật khẩu.", "Thành công");
            }
        }
    }

    private bool TryGetSelection(out UserSummary user)
    {
        UserSummary? selectedUser = _viewModel.SelectedUser;
        if (selectedUser is not null)
        {
            user = selectedUser;
            return true;
        }

        user = null!;
        MessageBox.Show("Vui lòng chọn một người dùng.", "Quản lý tài khoản");
        return false;
    }
}
