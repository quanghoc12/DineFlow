using System.Collections.ObjectModel;
using System.Globalization;
using DineFlow.BusinessObjects.Reports;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services.Api;
using DineFlow.WPFApp.Services.Authorization;

namespace DineFlow.WPFApp.Features.Reports.Revenue;

public sealed class RevenueReportViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;

    private DateTime _selectedDate;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    
    private decimal _totalRevenue;
    private decimal _cashRevenue;
    private decimal _bankTransferRevenue;
    private decimal _cardRevenue;

    public RevenueReportViewModel()
    {
        _apiClient = new StaffOrderApiClient();
        _selectedDate = DateTime.Today;
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                _ = LoadAsync();
            }
        }
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
                OnPropertyChanged(nameof(TotalRevenueText));
            }
        }
    }

    public decimal CashRevenue
    {
        get => _cashRevenue;
        private set
        {
            if (SetProperty(ref _cashRevenue, value))
            {
                OnPropertyChanged(nameof(CashRevenueText));
            }
        }
    }

    public decimal BankTransferRevenue
    {
        get => _bankTransferRevenue;
        private set
        {
            if (SetProperty(ref _bankTransferRevenue, value))
            {
                OnPropertyChanged(nameof(BankTransferRevenueText));
            }
        }
    }

    public decimal CardRevenue
    {
        get => _cardRevenue;
        private set
        {
            if (SetProperty(ref _cardRevenue, value))
            {
                OnPropertyChanged(nameof(CardRevenueText));
            }
        }
    }

    public string TotalRevenueText => FormatMoney(TotalRevenue);
    public string CashRevenueText => FormatMoney(CashRevenue);
    public string BankTransferRevenueText => FormatMoney(BankTransferRevenue);
    public string CardRevenueText => FormatMoney(CardRevenue);

    public bool IsAdmin => ApiClientSession.CurrentUserRole == "Admin";

    public ObservableCollection<BillHistoryRowViewModel> Bills { get; } = [];

    public async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            // 1. Tải tổng doanh thu
            RevenueSummaryDto summary = await _apiClient.GetRevenueSummaryAsync(SelectedDate, SelectedDate);
            TotalRevenue = summary.TotalRevenue;

            // 2. Tải doanh thu theo phương thức
            var methods = await _apiClient.GetRevenueByPaymentMethodAsync(SelectedDate, SelectedDate);
            decimal cash = 0, bank = 0, card = 0;
            foreach (var method in methods)
            {
                string m = method.PaymentMethod.ToLower();
                if (m.Contains("cash") || m.Contains("tiền mặt"))
                {
                    cash += method.TotalAmount;
                }
                else if (m.Contains("bank") || m.Contains("chuyển khoản"))
                {
                    bank += method.TotalAmount;
                }
                else if (m.Contains("card") || m.Contains("thẻ"))
                {
                    card += method.TotalAmount;
                }
                else
                {
                    // Fallback matching
                    if (method.PaymentMethod == DineFlow.BusinessObjects.Bills.PaymentMethods.Cash) cash += method.TotalAmount;
                    else if (method.PaymentMethod == DineFlow.BusinessObjects.Bills.PaymentMethods.BankTransfer) bank += method.TotalAmount;
                    else if (method.PaymentMethod == DineFlow.BusinessObjects.Bills.PaymentMethods.Card) card += method.TotalAmount;
                }
            }
            CashRevenue = cash;
            BankTransferRevenue = bank;
            CardRevenue = card;

            // 3. Tải danh sách hóa đơn đã thanh toán và gom nhóm
            var rawPayments = await _apiClient.GetPaidBillHistoryAsync(new PaidBillHistoryFilterDto { FromDate = SelectedDate, ToDate = SelectedDate });
            
            Bills.Clear();
            var grouped = rawPayments
                .GroupBy(x => x.BillId)
                .Select(g =>
                {
                    var first = g.First();
                    // Tạo chuỗi hiển thị chi tiết thanh toán nếu có nhiều phương thức
                    var paymentBreakdown = string.Join(", ", g.Select(p => $"{p.PaymentMethod}: {FormatMoney(p.PaymentAmount)}"));
                    return new BillHistoryRowViewModel
                    {
                        BillId = g.Key,
                        BillCode = first.BillCode,
                        TableName = first.TableName,
                        Area = first.Area,
                        PaidAt = first.PaidAt,
                        PaymentsText = paymentBreakdown,
                        FinalAmount = first.BillFinalAmount,
                        Payments = g.ToList()
                    };
                })
                .OrderByDescending(x => x.PaidAt)
                .ToList();

            foreach (var b in grouped)
            {
                Bills.Add(b);
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

    private static string FormatMoney(decimal amount) =>
        string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", amount);
}

public sealed class BillHistoryRowViewModel
{
    public int BillId { get; set; }
    public string BillCode { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public string PaidAtText => PaidAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    public string PaymentsText { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
    public string FinalAmountText => string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", FinalAmount);
    public List<PaidBillHistoryItemDto> Payments { get; set; } = [];
}
