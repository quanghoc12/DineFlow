using DineFlow.BusinessObjects.Auth;
using DineFlow.WPFApp.ViewModels;
using DineFlow.Services.Auth;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Views;

public partial class UserManagementView : UserControl
{
    private readonly UserManagementViewModel _viewModel;
    private readonly ICurrentUserService _currentUser;

    public UserManagementView(UserManagementViewModel viewModel, ICurrentUserService currentUser)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _currentUser = currentUser;
        DataContext = viewModel;
    }

    public Task LoadAsync() => _viewModel.LoadAsync();

    private async void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        UserEditorWindow dialog = new(null, _currentUser.User?.Role ?? AuthRoles.Staff)
            { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.CreateAsync(dialog.CreateRequest);
        }
    }

    private async void EditUserRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not UserSummary user) return;

        UserEditorWindow dialog = new(user, _currentUser.User?.Role ?? AuthRoles.Staff)
            { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.UpdateAsync(dialog.UpdateRequest);
            if (string.IsNullOrEmpty(_viewModel.ErrorMessage) &&
                dialog.DesiredIsActive != user.IsActive)
                await _viewModel.SetActiveAsync(user, dialog.DesiredIsActive);
            CloseManagementWindowWhenPermissionEnds();
        }
    }

    private async void ResetPasswordRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not UserSummary user) return;

        bool requireCurrentPassword = !AuthRoles.IsOwner(_currentUser.User!.Role);
        ResetPasswordWindow dialog = new(user.Username, requireCurrentPassword)
            { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.ResetPasswordAsync(user, dialog.CurrentPassword, dialog.NewPassword);
            if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                MessageBox.Show("Đã đặt lại mật khẩu.", "Thành công");
            }
        }
    }

    private void CloseManagementWindowWhenPermissionEnds()
    {
        if (_currentUser.IsAuthenticated && AuthRoles.CanManage(_currentUser.User?.Role)) return;
        Window.GetWindow(this)?.Close();
    }
}
