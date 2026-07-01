using System.Collections.ObjectModel;
using System.Globalization;
using DineFlow.BusinessObjects.Reports;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Reports.ViewModels;

public sealed class RevenueByPaymentMethodViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;

    private DateTime _fromDate;
    private DateTime _toDate;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public RevenueByPaymentMethodViewModel()
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

    public ObservableCollection<RevenueByPaymentMethodRowViewModel> Items { get; } = [];

    public async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            IReadOnlyList<PaymentMethodRevenueDto> items = await _apiClient.GetRevenueByPaymentMethodAsync(FromDate, ToDate);

            Items.Clear();
            foreach (PaymentMethodRevenueDto item in items)
            {
                Items.Add(new RevenueByPaymentMethodRowViewModel(item));
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

    public Task<byte[]> ExportCsvAsync() => _apiClient.ExportRevenueByPaymentMethodCsvAsync(FromDate, ToDate);
    public Task<byte[]> ExportExcelAsync() => _apiClient.ExportRevenueByPaymentMethodExcelAsync(FromDate, ToDate);

    public sealed class RevenueByPaymentMethodRowViewModel
    {
        public RevenueByPaymentMethodRowViewModel(PaymentMethodRevenueDto dto)
        {
            PaymentMethod = dto.PaymentMethod;
            PaymentCount = dto.PaymentCount;
            TotalAmount = dto.TotalAmount;
        }

        public string PaymentMethod { get; }
        public int PaymentCount { get; }
        public decimal TotalAmount { get; }
        public string TotalAmountText => string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", TotalAmount);
    }
}
