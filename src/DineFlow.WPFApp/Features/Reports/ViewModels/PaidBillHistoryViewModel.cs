using System.Collections.ObjectModel;
using System.Globalization;
using DineFlow.BusinessObjects.Reports;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Reports.ViewModels;

public sealed class PaidBillHistoryViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;

    private DateTime _fromDate;
    private DateTime _toDate;
    private string _selectedPaymentMethod = "All";
    private string _selectedTableName = "All";
    private string _selectedArea = "All";
    private string _keyword = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private decimal _totalPaymentAmount;

    public PaidBillHistoryViewModel()
    {
        _apiClient = new StaffOrderApiClient();
        DateTime today = DateTime.Today;
        _fromDate = new DateTime(today.Year, today.Month, 1);
        _toDate = today;

        PaymentMethods.Add("All");
        PaymentMethods.Add(DineFlow.BusinessObjects.Bills.PaymentMethods.Cash);
        PaymentMethods.Add(DineFlow.BusinessObjects.Bills.PaymentMethods.BankTransfer);
        PaymentMethods.Add(DineFlow.BusinessObjects.Bills.PaymentMethods.Card);

        TableNames.Add("All");
        Areas.Add("All");
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

    public string SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set => SetProperty(ref _selectedPaymentMethod, value);
    }

    public string SelectedTableName
    {
        get => _selectedTableName;
        set => SetProperty(ref _selectedTableName, value);
    }

    public string SelectedArea
    {
        get => _selectedArea;
        set => SetProperty(ref _selectedArea, value);
    }

    public string Keyword
    {
        get => _keyword;
        set => SetProperty(ref _keyword, value);
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

    public decimal TotalPaymentAmount
    {
        get => _totalPaymentAmount;
        private set
        {
            if (SetProperty(ref _totalPaymentAmount, value))
            {
                OnPropertyChanged(nameof(TotalPaymentAmountText));
            }
        }
    }

    public string TotalPaymentAmountText =>
        string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", TotalPaymentAmount);

    public ObservableCollection<string> PaymentMethods { get; } = [];
    public ObservableCollection<string> TableNames { get; } = [];
    public ObservableCollection<string> Areas { get; } = [];
    public ObservableCollection<PaidBillHistoryRowViewModel> Items { get; } = [];

    public int PaymentCount => Items.Count;

    public async Task LoadAsync()
    {
        ErrorMessage = string.Empty;

        IsBusy = true;

        try
        {
            await LoadFilterOptionsAsync();
            IReadOnlyList<PaidBillHistoryItemDto> items = await _apiClient.GetPaidBillHistoryAsync(BuildFilter());

            Items.Clear();
            foreach (PaidBillHistoryItemDto item in items)
            {
                Items.Add(new PaidBillHistoryRowViewModel(item));
            }

            TotalPaymentAmount = items.Sum(x => x.PaymentAmount);
            OnPropertyChanged(nameof(PaymentCount));
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            Items.Clear();
            TotalPaymentAmount = 0m;
            OnPropertyChanged(nameof(PaymentCount));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Task<byte[]> ExportCsvAsync() => _apiClient.ExportPaidBillHistoryCsvAsync(BuildFilter());

    public Task<byte[]> ExportExcelAsync() => _apiClient.ExportPaidBillHistoryExcelAsync(BuildFilter());

    private PaidBillHistoryFilterDto BuildFilter() =>
        new()
        {
            FromDate = FromDate,
            ToDate = ToDate,
            PaymentMethod = SelectedPaymentMethod,
            TableName = string.Equals(SelectedTableName, "All", StringComparison.OrdinalIgnoreCase) ? null : SelectedTableName,
            Area = string.Equals(SelectedArea, "All", StringComparison.OrdinalIgnoreCase) ? null : SelectedArea,
            Keyword = Keyword
        };

    private async Task LoadFilterOptionsAsync()
    {
        IReadOnlyList<DineFlow.Services.Orders.DiningTableDto> tables = await _apiClient.GetTablesAsync();

        string selectedTable = SelectedTableName;
        string selectedArea = SelectedArea;

        TableNames.Clear();
        TableNames.Add("All");
        foreach (string tableName in tables
                     .Select(x => x.TableName)
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            TableNames.Add(tableName);
        }

        Areas.Clear();
        Areas.Add("All");
        foreach (string area in tables
                     .Select(x => x.Area)
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            Areas.Add(area);
        }

        SelectedTableName = TableNames.Contains(selectedTable) ? selectedTable : "All";
        SelectedArea = Areas.Contains(selectedArea) ? selectedArea : "All";
    }

    public sealed class PaidBillHistoryRowViewModel
    {
        public PaidBillHistoryRowViewModel(PaidBillHistoryItemDto item)
        {
            PaymentId = item.PaymentId;
            BillId = item.BillId;
            BillCode = item.BillCode;
            BillName = item.BillName;
            TableName = item.TableName;
            Area = item.Area;
            PaymentMethod = item.PaymentMethod;
            PaymentAmount = item.PaymentAmount;
            BillFinalAmount = item.BillFinalAmount;
            PaidAt = item.PaidAt;
            ConfirmedByName = item.ConfirmedByName;
            UpdatedAt = item.UpdatedAt;
            UpdatedByName = item.UpdatedByName;
            ChangeReason = item.ChangeReason;
            IsCorrected = item.IsCorrected;
        }

        public int PaymentId { get; }
        public int BillId { get; }
        public string BillCode { get; }
        public string BillName { get; }
        public string TableName { get; }
        public string Area { get; }
        public string PaymentMethod { get; }
        public decimal PaymentAmount { get; }
        public decimal BillFinalAmount { get; }
        public DateTime PaidAt { get; }
        public string ConfirmedByName { get; }
        public DateTime? UpdatedAt { get; }
        public string UpdatedByName { get; }
        public string ChangeReason { get; }
        public bool IsCorrected { get; }

        public string PaymentAmountText => string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", PaymentAmount);
        public string BillFinalAmountText => string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", BillFinalAmount);
        public string PaidAtText => PaidAt.ToString("dd/MM/yyyy HH:mm");
        public string UpdatedAtText => UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
        public string CorrectedText => IsCorrected ? "Da chinh sua" : "Ban dau";

        public void OpenPaymentCorrection()
        {
            DashboardWorkspaceState.OpenPaymentCorrection(BillId);
        }
    }
}
