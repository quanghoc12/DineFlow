using System.Collections.ObjectModel;
using System.Globalization;
using DineFlow.BusinessObjects.Reports;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Reports.ViewModels;

public sealed class TopSellingItemsReportViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;

    private DateTime _fromDate;
    private DateTime _toDate;
    private int _topCount = 10;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public TopSellingItemsReportViewModel()
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

    public int TopCount
    {
        get => _topCount;
        set => SetProperty(ref _topCount, value);
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

    public ObservableCollection<TopSellingItemRowViewModel> Items { get; } = [];

    public async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            IReadOnlyList<TopSellingItemDto> items = await _apiClient.GetTopSellingItemsAsync(FromDate, ToDate, TopCount);

            Items.Clear();
            for (int index = 0; index < items.Count; index++)
            {
                Items.Add(new TopSellingItemRowViewModel(index + 1, items[index]));
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

    public Task<byte[]> ExportCsvAsync() => _apiClient.ExportTopSellingItemsCsvAsync(FromDate, ToDate, TopCount);
    public Task<byte[]> ExportExcelAsync() => _apiClient.ExportTopSellingItemsExcelAsync(FromDate, ToDate, TopCount);

    public sealed class TopSellingItemRowViewModel
    {
        public TopSellingItemRowViewModel(int rank, TopSellingItemDto item)
        {
            Rank = rank;
            MenuItemId = item.MenuItemId;
            ItemName = item.ItemName;
            TotalQuantity = item.TotalQuantity;
            TotalRevenue = item.TotalRevenue;
        }

        public int Rank { get; }
        public int MenuItemId { get; }
        public string ItemName { get; }
        public int TotalQuantity { get; }
        public decimal TotalRevenue { get; }
        public string TotalRevenueText => string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", TotalRevenue);
    }
}
