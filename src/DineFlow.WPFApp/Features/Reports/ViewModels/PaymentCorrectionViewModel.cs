using System.Collections.ObjectModel;
using System.Globalization;
using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Bills;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Reports.ViewModels;

public sealed class PaymentCorrectionViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;

    private string _billIdText = string.Empty;
    private string _selectedPaymentMethod = DineFlow.BusinessObjects.Bills.PaymentMethods.Cash;
    private string _changeReason = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;
    private BillDto? _loadedBill;
    private PaymentRowViewModel? _selectedPayment;

    public PaymentCorrectionViewModel()
    {
        _apiClient = new StaffOrderApiClient();
        PaymentMethods.Add(DineFlow.BusinessObjects.Bills.PaymentMethods.Cash);
        PaymentMethods.Add(DineFlow.BusinessObjects.Bills.PaymentMethods.BankTransfer);
        PaymentMethods.Add(DineFlow.BusinessObjects.Bills.PaymentMethods.Card);
    }

    public string BillIdText
    {
        get => _billIdText;
        set => SetProperty(ref _billIdText, value);
    }

    public string SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set => SetProperty(ref _selectedPaymentMethod, value);
    }

    public string ChangeReason
    {
        get => _changeReason;
        set => SetProperty(ref _changeReason, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        private set => SetProperty(ref _successMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool CanEdit => AuthRoles.CanManage(ApiClientSession.CurrentUserRole);
    public string CurrentRoleText => AuthRoles.GetLabel(ApiClientSession.CurrentUserRole);

    public string BillSummaryText =>
        _loadedBill is null
            ? "Chưa tải bill."
            : $"Bill {_loadedBill.BillId} - {_loadedBill.BillCode} - {_loadedBill.BillName} - {_loadedBill.FinalAmount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đ";

    public ObservableCollection<string> PaymentMethods { get; } = [];
    public ObservableCollection<PaymentRowViewModel> Payments { get; } = [];

    public void SetBillId(int billId)
    {
        BillIdText = billId.ToString(CultureInfo.InvariantCulture);
    }

    public PaymentRowViewModel? SelectedPayment
    {
        get => _selectedPayment;
        set
        {
            if (SetProperty(ref _selectedPayment, value) && value is not null)
            {
                SelectedPaymentMethod = value.PaymentMethod;
            }
        }
    }

    public async Task LoadBillAsync()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!CanEdit)
        {
            ErrorMessage = "Chỉ Admin hoặc Chủ nhà hàng được sửa payment method.";
            return;
        }

        if (!int.TryParse(BillIdText.Trim(), out int billId))
        {
            ErrorMessage = "BillId phải là số hợp lệ.";
            return;
        }

        IsBusy = true;
        try
        {
            BillDto? bill = await _apiClient.GetBillAsync(billId);
            if (bill is null)
            {
                throw new InvalidOperationException("Không tìm thấy bill.");
            }

            if (!string.Equals(bill.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Chỉ bill đã thanh toán mới được sửa payment method.");
            }

            _loadedBill = bill;
            Payments.Clear();
            foreach (PaymentDto payment in bill.Payments.OrderBy(x => x.PaidAt).ThenBy(x => x.PaymentId))
            {
                Payments.Add(new PaymentRowViewModel(payment));
            }

            SelectedPayment = Payments.FirstOrDefault();
            OnPropertyChanged(nameof(BillSummaryText));
        }
        catch (Exception exception)
        {
            _loadedBill = null;
            Payments.Clear();
            SelectedPayment = null;
            OnPropertyChanged(nameof(BillSummaryText));
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ApplyCorrectionAsync()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!CanEdit)
        {
            ErrorMessage = "Chỉ Admin hoặc Chủ nhà hàng được sửa payment method.";
            return;
        }

        if (_loadedBill is null)
        {
            ErrorMessage = "Hãy tải bill trước.";
            return;
        }

        if (SelectedPayment is null)
        {
            ErrorMessage = "Hãy chọn payment cần sửa.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ChangeReason))
        {
            ErrorMessage = "Lý do thay đổi là bắt buộc.";
            return;
        }

        IsBusy = true;
        try
        {
            await _apiClient.UpdatePaidPaymentMethodAsync(
                _loadedBill.BillId,
                new UpdatePaidPaymentMethodRequest
                {
                    PaymentId = SelectedPayment.PaymentId,
                    NewPaymentMethod = SelectedPaymentMethod,
                    ChangeReason = ChangeReason.Trim()
                });

            SuccessMessage = "Đã cập nhật payment method thành công.";
            await LoadBillAsync();
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

    public sealed class PaymentRowViewModel
    {
        public PaymentRowViewModel(PaymentDto payment)
        {
            PaymentId = payment.PaymentId;
            PaymentMethod = payment.PaymentMethod;
            Amount = payment.Amount;
            PaidAt = payment.PaidAt;
            ConfirmedBy = payment.ConfirmedBy;
            UpdatedAt = payment.UpdatedAt;
            UpdatedBy = payment.UpdatedBy;
            ChangeReason = payment.ChangeReason ?? string.Empty;
        }

        public int PaymentId { get; }
        public string PaymentMethod { get; }
        public decimal Amount { get; }
        public DateTime PaidAt { get; }
        public int ConfirmedBy { get; }
        public DateTime? UpdatedAt { get; }
        public int? UpdatedBy { get; }
        public string ChangeReason { get; }
        public string AmountText => string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", Amount);
        public string PaidAtText => PaidAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        public string UpdatedAtText => UpdatedAt?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
    }
}
