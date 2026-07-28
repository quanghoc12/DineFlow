using System.Windows;
using System.Windows.Controls;
using DineFlow.BusinessObjects.Reports;

namespace DineFlow.WPFApp.Features.Dashboard;

public partial class DashboardView : UserControl
{
    private const int OverviewIndex = 0;
    private const int RevenueIndex = 1;
    private const int CancellationIndex = 2;

    private readonly DashboardViewModel _viewModel;
    private readonly DashboardAssistantViewModel _assistantViewModel;

    public DashboardView(
        DashboardViewModel viewModel,
        DashboardAssistantViewModel assistantViewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _assistantViewModel = assistantViewModel;
        DataContext = _viewModel;
        Loaded += DashboardView_Loaded;
        Unloaded += DashboardView_Unloaded;
    }

    public Task LoadAsync() => _viewModel.LoadAsync();

    private void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        DashboardWorkspaceState.NavigationRequested += OnNavigationRequested;
    }

    private void DashboardView_Unloaded(object sender, RoutedEventArgs e)
    {
        DashboardWorkspaceState.NavigationRequested -= OnNavigationRequested;
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        _assistantViewModel.InvalidateDataCache();
        await LoadAsync();
    }

    private void OpenAssistantButton_Click(object sender, RoutedEventArgs e)
    {
        _assistantViewModel.UpdateContext(BuildAssistantContext());

        DashboardAssistantWindow window = new(_assistantViewModel)
        {
            Owner = Window.GetWindow(this)
        };
        window.ShowDialog();
    }

    private void OnNavigationRequested(DashboardWorkspaceNavigationRequest request)
    {
        Dispatcher.Invoke(() => SetActiveTab(request.TabKey switch
        {
            DashboardTabs.Overview => OverviewIndex,
            DashboardTabs.Revenue => RevenueIndex,
            DashboardTabs.Cancellation => CancellationIndex,
            _ => OverviewIndex
        }));
    }

    private void OpenOverviewTabButton_Click(object sender, RoutedEventArgs e) => SetActiveTab(OverviewIndex);
    private void OpenRevenueTabButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTab(RevenueIndex);
        // Tải lại doanh thu ngày hôm nay khi chuyển sang tab doanh thu
        _ = RevenueReportViewControl.LoadTodayRevenueAsync();
    }
    
    private void OpenCancellationTabButton_Click(object sender, RoutedEventArgs e)
    {
        SetActiveTab(CancellationIndex);
        // Tải lại thống kê hủy ngày hôm nay khi chuyển sang tab hủy
        _ = CancellationViewControl.LoadTodayCancellationsAsync();
    }

    private void SetActiveTab(int tabIndex)
    {
        OverviewPanel.Visibility = tabIndex == OverviewIndex ? Visibility.Visible : Visibility.Collapsed;
        RevenuePanel.Visibility = tabIndex == RevenueIndex ? Visibility.Visible : Visibility.Collapsed;
        CancellationPanel.Visibility = tabIndex == CancellationIndex ? Visibility.Visible : Visibility.Collapsed;

        OverviewTabButton.Tag = tabIndex == OverviewIndex ? "Active" : null;
        RevenueTabButton.Tag = tabIndex == RevenueIndex ? "Active" : null;
        CancellationTabButton.Tag = tabIndex == CancellationIndex ? "Active" : null;
    }

    private DashboardAssistantContextDto BuildAssistantContext()
    {
        string currentTab = RevenuePanel.Visibility == Visibility.Visible
            ? DashboardTabs.Revenue
            : CancellationPanel.Visibility == Visibility.Visible
                ? DashboardTabs.Cancellation
                : DashboardTabs.Overview;

        return new DashboardAssistantContextDto
        {
            CurrentTab = currentTab,
            ChartMode = _viewModel.SelectedChartMode.ToString(),
            TopSellingPeriod = _viewModel.TopSellingPeriod,
            PaymentPeriod = _viewModel.PaymentPeriod
        };
    }
}
