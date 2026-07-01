using System.Collections.ObjectModel;
using System.Globalization;
using DineFlow.BusinessObjects.Reports;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Reports.ViewModels;

public sealed class DashboardViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;

    private string _titleDate = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private decimal _revenueToday;
    private int _paidBillCountToday;
    private decimal _averageBillValue;
    private int _orderCountToday;
    private int _servingTableCount;
    private int _waitingPaymentTableCount;
    private int _printFailedOrderCount;

    public DashboardViewModel()
    {
        _apiClient = new StaffOrderApiClient();
    }

    public string TitleDate
    {
        get => _titleDate;
        private set => SetProperty(ref _titleDate, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public decimal RevenueToday
    {
        get => _revenueToday;
        private set
        {
            if (SetProperty(ref _revenueToday, value))
            {
                OnComputedChanged();
            }
        }
    }

    public int PaidBillCountToday
    {
        get => _paidBillCountToday;
        private set => SetProperty(ref _paidBillCountToday, value);
    }

    public decimal AverageBillValue
    {
        get => _averageBillValue;
        private set
        {
            if (SetProperty(ref _averageBillValue, value))
            {
                OnComputedChanged();
            }
        }
    }

    public int OrderCountToday
    {
        get => _orderCountToday;
        private set => SetProperty(ref _orderCountToday, value);
    }

    public int ServingTableCount
    {
        get => _servingTableCount;
        private set => SetProperty(ref _servingTableCount, value);
    }

    public int WaitingPaymentTableCount
    {
        get => _waitingPaymentTableCount;
        private set => SetProperty(ref _waitingPaymentTableCount, value);
    }

    public int PrintFailedOrderCount
    {
        get => _printFailedOrderCount;
        private set => SetProperty(ref _printFailedOrderCount, value);
    }

    public string RevenueTodayText => FormatMoney(RevenueToday);
    public string AverageBillValueText => FormatMoney(AverageBillValue);

    public ObservableCollection<TopSellingItemDto> TopSellingItems { get; } = [];
    public ObservableCollection<PaymentMethodRevenueDto> RevenueByPaymentMethods { get; } = [];

    public async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            DashboardDto dashboard = await _apiClient.GetTodayDashboardAsync();
            TitleDate = dashboard.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            RevenueToday = dashboard.RevenueToday;
            PaidBillCountToday = dashboard.PaidBillCountToday;
            AverageBillValue = dashboard.AverageBillValue;
            OrderCountToday = dashboard.OrderCountToday;
            ServingTableCount = dashboard.ServingTableCount;
            WaitingPaymentTableCount = dashboard.WaitingPaymentTableCount;
            PrintFailedOrderCount = dashboard.PrintFailedOrderCount;

            TopSellingItems.Clear();
            foreach (TopSellingItemDto item in dashboard.TopSellingItems)
            {
                TopSellingItems.Add(item);
            }

            RevenueByPaymentMethods.Clear();
            foreach (PaymentMethodRevenueDto item in dashboard.RevenueByPaymentMethods)
            {
                RevenueByPaymentMethods.Add(item);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnComputedChanged()
    {
        OnPropertyChanged(nameof(RevenueTodayText));
        OnPropertyChanged(nameof(AverageBillValueText));
    }

    private static string FormatMoney(decimal amount) =>
        string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", amount);
}
