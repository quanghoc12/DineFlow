using System.Windows;
using System.Windows.Controls;
using DineFlow.WPFApp.Features.Reports.ViewModels;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Reports.Payments;

public partial class PaymentCorrectionView : UserControl
{
    private readonly PaymentCorrectionViewModel _viewModel;

    public PaymentCorrectionView()
        : this(new PaymentCorrectionViewModel())
    {
    }

    public PaymentCorrectionView(PaymentCorrectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += PaymentCorrectionView_Loaded;
        Unloaded += PaymentCorrectionView_Unloaded;
    }

    private void PaymentCorrectionView_Loaded(object sender, RoutedEventArgs e)
    {
        DashboardWorkspaceState.NavigationRequested += OnNavigationRequested;
    }

    private void PaymentCorrectionView_Unloaded(object sender, RoutedEventArgs e)
    {
        DashboardWorkspaceState.NavigationRequested -= OnNavigationRequested;
    }

    private async void OnNavigationRequested(DashboardWorkspaceNavigationRequest request)
    {
        if (request.TabKey != DashboardTabs.PaymentCorrection || !request.BillId.HasValue)
        {
            return;
        }

        Dispatcher.Invoke(() => _viewModel.SetBillId(request.BillId.Value));
        await _viewModel.LoadBillAsync();
    }

    private async void LoadBillButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadBillAsync();
    }

    private async void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.ApplyCorrectionAsync();
    }
}
