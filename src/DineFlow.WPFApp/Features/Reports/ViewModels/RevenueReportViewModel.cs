using System.Collections.ObjectModel;
using System.Globalization;
using DineFlow.BusinessObjects.Reports;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Reports.ViewModels;

public sealed class RevenueReportViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;

    private DateTime _fromDate;
    private DateTime _toDate;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private decimal _totalRevenue;
    private int _paidBillCount;
    private decimal _averageBillValue;

    public RevenueReportViewModel()
    {
        _apiClient = new StaffOrderApiClient();
        DateTime today = DateTime.Today;
        _fromDate = new DateTime(today.Year, today.Month, 1);
        _toDate = today;
    }

    public DateTime FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
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

    public decimal TotalRevenue
    {
        get => _totalRevenue;
        private set
        {
            if (SetProperty(ref _totalRevenue, value))
            {
                OnComputedChanged();
            }
        }
    }

    public int PaidBillCount
    {
        get => _paidBillCount;
        private set => SetProperty(ref _paidBillCount, value);
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

    public string TotalRevenueText => FormatMoney(TotalRevenue);
    public string AverageBillValueText => FormatMoney(AverageBillValue);

    public ObservableCollection<RevenueByDayDto> RevenueByDays { get; } = [];

    public async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            RevenueSummaryDto summary = await _apiClient.GetRevenueSummaryAsync(FromDate, ToDate);
            TotalRevenue = summary.TotalRevenue;
            PaidBillCount = summary.PaidBillCount;
            AverageBillValue = summary.AverageBillValue;

            RevenueByDays.Clear();
            foreach (RevenueByDayDto item in summary.RevenueByDays)
            {
                RevenueByDays.Add(item);
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

    public Task<byte[]> ExportCsvAsync() => _apiClient.ExportRevenueSummaryCsvAsync(FromDate, ToDate);
    public Task<byte[]> ExportExcelAsync() => _apiClient.ExportRevenueSummaryExcelAsync(FromDate, ToDate);

    private void OnComputedChanged()
    {
        OnPropertyChanged(nameof(TotalRevenueText));
        OnPropertyChanged(nameof(AverageBillValueText));
    }

    private static string FormatMoney(decimal amount) =>
        string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", amount);
}
