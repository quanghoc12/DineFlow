using System.Windows;
using System.Windows.Controls;
using DineFlow.WPFApp.Features.Reports.ViewModels;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Reports.Dashboard;

public partial class DashboardView : UserControl
{
    private const int OverviewIndex = 0;
    private const int RevenueIndex = 1;
    private const int TopSellingIndex = 2;
    private const int PaymentMethodIndex = 3;
    private const int PaidHistoryIndex = 4;
    private const int PaymentCorrectionIndex = 5;

    private readonly DashboardViewModel _viewModel;

    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
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
        await LoadAsync();
    }

    private void OnNavigationRequested(DashboardWorkspaceNavigationRequest request)
    {
        Dispatcher.Invoke(() => SetActiveTab(request.TabKey switch
        {
            DashboardTabs.Overview => OverviewIndex,
            DashboardTabs.Revenue => RevenueIndex,
            DashboardTabs.TopSelling => TopSellingIndex,
            DashboardTabs.PaymentMethodRevenue => PaymentMethodIndex,
            DashboardTabs.PaidBillHistory => PaidHistoryIndex,
            DashboardTabs.PaymentCorrection => PaymentCorrectionIndex,
            _ => OverviewIndex
        }));
    }

    private void OpenOverviewTabButton_Click(object sender, RoutedEventArgs e) => SetActiveTab(OverviewIndex);
    private void OpenRevenueTabButton_Click(object sender, RoutedEventArgs e) => SetActiveTab(RevenueIndex);
    private void OpenTopSellingTabButton_Click(object sender, RoutedEventArgs e) => SetActiveTab(TopSellingIndex);
    private void OpenPaymentMethodRevenueTabButton_Click(object sender, RoutedEventArgs e) => SetActiveTab(PaymentMethodIndex);
    private void OpenPaidBillHistoryTabButton_Click(object sender, RoutedEventArgs e) => SetActiveTab(PaidHistoryIndex);
    private void OpenPaymentCorrectionTabButton_Click(object sender, RoutedEventArgs e) => SetActiveTab(PaymentCorrectionIndex);

    private void SetActiveTab(int tabIndex)
    {
        OverviewPanel.Visibility = tabIndex == OverviewIndex ? Visibility.Visible : Visibility.Collapsed;
        RevenuePanel.Visibility = tabIndex == RevenueIndex ? Visibility.Visible : Visibility.Collapsed;
        TopSellingPanel.Visibility = tabIndex == TopSellingIndex ? Visibility.Visible : Visibility.Collapsed;
        PaymentMethodPanel.Visibility = tabIndex == PaymentMethodIndex ? Visibility.Visible : Visibility.Collapsed;
        PaidHistoryPanel.Visibility = tabIndex == PaidHistoryIndex ? Visibility.Visible : Visibility.Collapsed;
        PaymentCorrectionPanel.Visibility = tabIndex == PaymentCorrectionIndex ? Visibility.Visible : Visibility.Collapsed;

        OverviewTabButton.Tag = tabIndex == OverviewIndex ? "Active" : null;
        RevenueTabButton.Tag = tabIndex == RevenueIndex ? "Active" : null;
        TopSellingTabButton.Tag = tabIndex == TopSellingIndex ? "Active" : null;
        PaymentMethodTabButton.Tag = tabIndex == PaymentMethodIndex ? "Active" : null;
        PaidHistoryTabButton.Tag = tabIndex == PaidHistoryIndex ? "Active" : null;
        PaymentCorrectionTabButton.Tag = tabIndex == PaymentCorrectionIndex ? "Active" : null;
    }
}
