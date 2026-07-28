using System.Windows;
using System.Windows.Controls;
using DineFlow.WPFApp.Features.Operations.OrderManagement;
using DineFlow.WPFApp.Services.Api;
using DineFlow.WPFApp.Services.Authorization;
using DineFlow.WPFApp.Services.Realtime;
using DineFlow.Services.Auth;
using DineFlow.WPFApp.Features.Management.Tables;
using DineFlow.WPFApp.Features.Management.Users;
using DineFlow.WPFApp.Features.Management.Menu;
using DineFlow.BusinessObjects.Auth;
using DineFlow.WPFApp.Features.Dashboard;
using Microsoft.Extensions.DependencyInjection;

namespace DineFlow.WPFApp;

public partial class MainWindow : Window
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthService _authService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OrderManagementView _orderManagementView;
    private readonly List<IServiceScope> _featureScopes = [];

    public MainWindow(
        ICurrentUserService currentUserService,
        IAuthService authService,
        IServiceScopeFactory scopeFactory)
    {
        _currentUserService = currentUserService;
        _authService = authService;
        _scopeFactory = scopeFactory;
        _orderManagementView = new OrderManagementView(
            new StaffOrderApiClient(),
            new StaffRealtimeClient(),
            new PdfDemoPrintService());
        _orderManagementView.SidebarNotificationCountChanged += UpdateOrderSidebarBadge;
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
        Closed += MainWindow_Closed;
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
        DashboardView dashboardView = ShowScopedView<DashboardView>(DashboardButton);
        await dashboardView.LoadAsync();
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

        MenuManagementView menuManagementView = ShowScopedView<MenuManagementView>(MenuButton);
        await menuManagementView.LoadAsync();
    }

    private async void TableButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasManagementRole())
        {
            MessageBox.Show("Chỉ Admin hoặc Chủ nhà hàng được quản lý bàn và mã QR.", "Không có quyền");
            return;
        }

        TableManagementView tableManagementView = ShowScopedView<TableManagementView>(TableButton);
        await tableManagementView.LoadAsync();
    }

    private async void AccountButton_Click(object sender, RoutedEventArgs e)
    {
        if (!HasManagementRole())
        {
            MessageBox.Show("Chỉ Admin hoặc Chủ nhà hàng được quản lý người dùng.", "Không có quyền");
            return;
        }

        UserManagementView userManagementView = ShowScopedView<UserManagementView>(AccountButton);
        await userManagementView.LoadAsync();
    }

    private bool HasManagementRole() => AuthRoles.CanManage(_currentUserService.User?.Role);

    private void ShowOrderScreen()
    {
        DisposeFeatureScope();
        ScreenHost.Content = _orderManagementView;
        SetActiveButton(OrderButton);
    }

    private async void MainWindow_Closed(object? sender, EventArgs e)
    {
        _orderManagementView.SidebarNotificationCountChanged -= UpdateOrderSidebarBadge;
        DisposeFeatureScope();
        await _orderManagementView.DisposeAsync();
    }

    private TView ShowScopedView<TView>(Button activeButton)
        where TView : UserControl
    {
        IServiceScope scope = _scopeFactory.CreateScope();
        _featureScopes.Add(scope);
        TView view = scope.ServiceProvider.GetRequiredService<TView>();
        ScreenHost.Content = view;
        SetActiveButton(activeButton);
        return view;
    }

    private void DisposeFeatureScope()
    {
        foreach (IServiceScope scope in _featureScopes.ToList())
        {
            try
            {
                scope.Dispose();
            }
            catch
            {
                // A screen may still be unwinding an async DB connection; app shutdown should continue.
            }
        }

        _featureScopes.Clear();
    }

    private void UpdateOrderSidebarBadge(int count)
    {
        Dispatcher.Invoke(() =>
        {
            OrderSidebarBadge.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
            OrderSidebarBadgeText.Text = count > 99 ? "99+" : count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });
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
