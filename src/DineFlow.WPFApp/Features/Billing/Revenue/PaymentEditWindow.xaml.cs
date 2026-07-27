using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using DineFlow.BusinessObjects.Reports;
using DineFlow.Services.Bills;
using DineFlow.WPFApp.Services.Api;

namespace DineFlow.WPFApp.Features.Billing.Revenue;

public partial class PaymentEditWindow : Window
{
    private readonly BillHistoryRowViewModel _billRow;
    private readonly StaffOrderApiClient _apiClient;
    private readonly ObservableCollection<EditablePaymentRow> _editablePayments = [];

    public PaymentEditWindow(BillHistoryRowViewModel billRow)
    {
        InitializeComponent();
        _billRow = billRow;
        _apiClient = new StaffOrderApiClient();

        BillTitleText.Text = $"Hóa đơn: {_billRow.BillCode} - {_billRow.TableName}";
        BillTotalText.Text = $"Tổng tiền hóa đơn: {billRow.FinalAmountText}";

        foreach (var payment in _billRow.Payments)
        {
            _editablePayments.Add(new EditablePaymentRow
            {
                PaymentId = payment.PaymentId,
                PaymentMethod = payment.PaymentMethod,
                AmountText = ((int)payment.PaymentAmount).ToString(CultureInfo.InvariantCulture)
            });
        }

        PaymentsList.ItemsSource = _editablePayments;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;
        string reason = ReasonTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(reason))
        {
            ShowError("Vui lòng nhập lý do thay đổi.");
            return;
        }

        // Validate và tính tổng tiền
        decimal totalAmount = 0;
        var updateParts = new List<PaymentUpdatePart>();

        foreach (var row in _editablePayments)
        {
            if (string.IsNullOrWhiteSpace(row.AmountText))
            {
                ShowError("Số tiền thanh toán không được để trống.");
                return;
            }

            if (!decimal.TryParse(row.AmountText.Replace(",", "").Replace(".", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) || amount < 0)
            {
                ShowError("Số tiền thanh toán không hợp lệ (phải là số lớn hơn hoặc bằng 0).");
                return;
            }

            totalAmount += amount;
            updateParts.Add(new PaymentUpdatePart
            {
                PaymentId = row.PaymentId,
                PaymentMethod = row.PaymentMethod,
                Amount = amount
            });
        }

        if (totalAmount != _billRow.FinalAmount)
        {
            string expectedText = string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", _billRow.FinalAmount);
            string actualText = string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} đ", totalAmount);
            ShowError($"Tổng số tiền điều chỉnh ({actualText}) không khớp với tổng tiền hóa đơn ({expectedText}).");
            return;
        }

        try
        {
            await _apiClient.BatchUpdatePaymentsAsync(_billRow.BillId, new BatchUpdatePaymentsRequest
            {
                ChangeReason = reason,
                Payments = updateParts
            });

            MessageBox.Show("Cập nhật phương thức thanh toán thành công.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    public class EditablePaymentRow
    {
        public int PaymentId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string AmountText { get; set; } = string.Empty;
    }
}
