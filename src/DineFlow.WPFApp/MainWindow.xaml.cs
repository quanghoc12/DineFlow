using System.Windows;
using System.Windows.Controls;
using DineFlow.WPFApp.Features.Operations.OrderManagement;
using DineFlow.WPFApp.Services;
using DineFlow.Services.Auth;
using DineFlow.WPFApp.Views;
using DineFlow.WPFApp.Features.Management.Tables;
using DineFlow.WPFApp.Features.Management.Menu;

namespace DineFlow.WPFApp;

public partial class MainWindow : Window
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthService _authService;
    private readonly UserManagementView _userManagementView;
    private readonly TableManagementView _tableManagementView;
    private readonly MenuManagementView _menuManagementView;

    public MainWindow(
        ICurrentUserService currentUserService,
        IAuthService authService,
        UserManagementView userManagementView,
        TableManagementView tableManagementView,
        MenuManagementView menuManagementView)
    {
        _currentUserService = currentUserService;
        _authService = authService;
        _userManagementView = userManagementView;
        _tableManagementView = tableManagementView;
        _menuManagementView = menuManagementView;
        InitializeComponent();
        CurrentUserText.Text = string.IsNullOrWhiteSpace(_currentUserService.User?.FullName)
            ? _currentUserService.User?.Username ?? string.Empty
            : $"{_currentUserService.User.FullName} ({_currentUserService.User.Role})";
        bool isAdmin = _currentUserService.User?.Role.Equals(
            "Admin",
            StringComparison.OrdinalIgnoreCase) == true;
        AccountButton.IsEnabled = isAdmin;
        AccountButton.ToolTip = isAdmin
            ? "Quản lý tài khoản"
            : "Chỉ Admin được sử dụng chức năng này";
        TableButton.IsEnabled = isAdmin;
        TableButton.ToolTip = isAdmin
            ? "Quản lý bàn và mã QR"
            : "Chỉ Admin được sử dụng chức năng này";
        MenuButton.IsEnabled = isAdmin;
        MenuButton.ToolTip = isAdmin
            ? "Quản lý thực đơn"
            : "Chỉ Admin được sử dụng chức năng này";
        Loaded += MainWindow_Loaded;
        ShowOrderScreen();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Bạn có chắc muốn đăng xuất?",
            "Đăng xuất",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _authService.Logout();
        Close();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Rect workArea = SystemParameters.WorkArea;
        WindowState = WindowState.Normal;
        Left = workArea.Left;
        Top = workArea.Top;
        Width = workArea.Width;
        Height = workArea.Height;
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e)
    {
        SetPlaceholderScreen("Dashboard", "Tổng quan doanh thu, bàn đang phục vụ và đơn đang chờ.");
        SetActiveButton(DashboardButton);
    }

    private void OrderButton_Click(object sender, RoutedEventArgs e)
    {
        ShowOrderScreen();
    }

    private async void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.User!.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Chỉ Admin được quản lý thực đơn.", "Không có quyền");
            return;
        }

        ScreenHost.Content = _menuManagementView;
        SetActiveButton(MenuButton);
        await _menuManagementView.LoadAsync();
    }

    private async void TableButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.User!.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Chỉ Admin được quản lý bàn và mã QR.", "Không có quyền");
            return;
        }

        ScreenHost.Content = _tableManagementView;
        SetActiveButton(TableButton);
        await _tableManagementView.LoadAsync();
    }

    private async void AccountButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_currentUserService.User!.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Chỉ Admin được quản lý người dùng.", "Không có quyền");
            return;
        }

        ScreenHost.Content = _userManagementView;
        SetActiveButton(AccountButton);
        await _userManagementView.LoadAsync();
    }

    private void ShowOrderScreen()
    {
        ScreenHost.Content = new OrderManagementView(
            new StaffOrderApiClient(),
            new StaffRealtimeClient(),
            new PdfDemoPrintService());
        SetActiveButton(OrderButton);
    }

    private void SetPlaceholderScreen(string title, string subtitle)
    {
        ScreenHost.Content = new Border
        {
            Background = System.Windows.Media.Brushes.White,
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(28),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.Black
                    },
                    new TextBlock
                    {
                        Text = subtitle,
                        FontSize = 15,
                        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(82, 99, 118)),
                        Margin = new Thickness(0, 8, 0, 0)
                    }
                }
            }
        };
    }

    private void SetActiveButton(Button activeButton)
    {
        DashboardButton.Tag = null;
        OrderButton.Tag = null;
        MenuButton.Tag = null;
        TableButton.Tag = null;
        AccountButton.Tag = null;
        activeButton.Tag = "Active";
    }
}
