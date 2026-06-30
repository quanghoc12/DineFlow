using System.Windows;
using System.Windows.Controls;
using DineFlow.WPFApp.Features.Operations.OrderManagement;
using DineFlow.WPFApp.Services;
using DineFlow.Services.Auth;
using DineFlow.Services.Bills;
using DineFlow.Services.Menu;
using DineFlow.WPFApp.Views;
using DineFlow.WPFApp.Features.Management.Tables;
using DineFlow.WPFApp.Features.Management.Menu;
using DineFlow.BusinessObjects.Auth;
using DineFlow.WPFApp.Features.Reports.Dashboard;

namespace DineFlow.WPFApp;

public partial class MainWindow : Window
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthService _authService;
    private readonly IMenuManagementService _menuManagementService;
    private readonly IBillService _billService;
    private readonly UserManagementView _userManagementView;
    private readonly TableManagementView _tableManagementView;
    private readonly MenuManagementView _menuManagementView;
    private readonly DashboardView _dashboardView;

    public MainWindow(
        ICurrentUserService currentUserService,
        IAuthService authService,
        IMenuManagementService menuManagementService,
        IBillService billService,
        UserManagementView userManagementView,
        TableManagementView tableManagementView,
        MenuManagementView menuManagementView,
        DashboardView dashboardView)
    {
        _currentUserService = currentUserService;
        _authService = authService;
        _menuManagementService = menuManagementService;
        _billService = billService;
        _userManagementView = userManagementView;
        _tableManagementView = tableManagementView;
        _menuManagementView = menuManagementView;
        _dashboardView = dashboardView;
        InitializeComponent();
        CurrentUserText.Text = string.IsNullOrWhiteSpace(_currentUserService.User?.FullName)
            ? _currentUserService.User?.Username ?? string.Empty
            : $"{_currentUserService.User.FullName} ({_currentUserService.User.Role})";
        bool canManage = HasManagementRole();
        AccountButton.IsEnabled = canManage;
        AccountButton.ToolTip = canManage
            ? "Quản lý tài khoản"
            : "Chỉ Admin hoặc Chủ nhà hàng được sử dụng chức năng này";
        TableButton.IsEnabled = canManage;
        TableButton.ToolTip = canManage
            ? "Quản lý bàn và mã QR"
            : "Chỉ Admin hoặc Chủ nhà hàng được sử dụng chức năng này";
        MenuButton.IsEnabled = canManage;
        MenuButton.ToolTip = canManage
            ? "Quản lý thực đơn"
            : "Chỉ Admin hoặc Chủ nhà hàng được sử dụng chức năng này";
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
        ApiClientSession.Clear();
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

    private async void DashboardButton_Click(object sender, RoutedEventArgs e)
    {
        ScreenHost.Content = _dashboardView;
        SetActiveButton(DashboardButton);
        await _dashboardView.LoadAsync();
    }

    private void OrderButton_Click(object sender, RoutedEventArgs e)
    {
        ShowOrderScreen();
    }

    private async void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasManagementRole())
        {
            MessageBox.Show("Chỉ Admin hoặc Chủ nhà hàng được quản lý thực đơn.", "Không có quyền");
            return;
        }

        ScreenHost.Content = _menuManagementView;
        SetActiveButton(MenuButton);
        await _menuManagementView.LoadAsync();
    }

    private async void TableButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasManagementRole())
        {
            MessageBox.Show("Chỉ Admin hoặc Chủ nhà hàng được quản lý bàn và mã QR.", "Không có quyền");
            return;
        }

        ScreenHost.Content = _tableManagementView;
        SetActiveButton(TableButton);
        await _tableManagementView.LoadAsync();
    }

    private async void AccountButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasManagementRole())
        {
            MessageBox.Show("Chỉ Admin hoặc Chủ nhà hàng được quản lý người dùng.", "Không có quyền");
            return;
        }

        ScreenHost.Content = _userManagementView;
        SetActiveButton(AccountButton);
        await _userManagementView.LoadAsync();
    }

    private bool HasManagementRole() => AuthRoles.CanManage(_currentUserService.User?.Role);

    private void ShowOrderScreen()
    {
        ScreenHost.Content = new OrderManagementView(
            new StaffOrderApiClient(),
            new StaffRealtimeClient(),
            new PdfDemoPrintService(),
            _menuManagementService,
            _billService);
        SetActiveButton(OrderButton);
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
