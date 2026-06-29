using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DineFlow.Services.Bills;
using DineFlow.Services.Menu;
using DineFlow.Services.Orders;
using DineFlow.Services.Requests;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{
    private void PrintKitchenBillCancelPdf(BillPreview bill, string reason)
    {
        string path = _pdfPrintService.PrintKitchenBillCancel(_selectedTable, bill, reason);
        ShowCustomMessageBox($"Đã tạo PDF hủy bill cho bếp:\n{path}", "Hủy bill", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PrintTemporaryBillButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBill is null || _selectedBill.Lines.Count == 0)
        {
            ShowCustomMessageBox("Bill chưa có món để in tạm tính.", "In tạm tính", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        string path = _pdfPrintService.PrintTemporaryBill(_selectedTable, _selectedBill);
        ShowCustomMessageBox($"Đã tạo PDF tạm tính:\n{path}", "In tạm tính", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PrintKitchenOrderPdfIfNeeded(IReadOnlyList<(BillLinePreview Line, int Quantity)> kitchenLines)
    {
        if (_selectedBill is null || kitchenLines.Count == 0)
        {
            return;
        }

        string path = _pdfPrintService.PrintKitchenOrder(_selectedTable, _selectedBill, kitchenLines);
        ShowCustomMessageBox($"Đã tạo PDF phiếu bếp:\n{path}", "Thông báo bếp", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PrintPaymentReceiptPdf(BillPreview bill, PaymentDialogResult payment, string paymentMethods)
    {
        string path = _pdfPrintService.PrintPaymentReceipt(
            _selectedTable,
            bill,
            payment.Parts.Sum(x => x.Amount),
            paymentMethods);

        ShowCustomMessageBox($"Đã tạo PDF thanh toán:\n{path}", "Thanh toán", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PrintKitchenCancelPdf(BillLinePreview line, int cancelQuantity, string reason)
    {
        if (_selectedBill is null || cancelQuantity <= 0)
        {
            return;
        }

        string path = _pdfPrintService.PrintKitchenCancel(_selectedTable, _selectedBill, line, cancelQuantity, reason);
        ShowCustomMessageBox($"Đã tạo PDF hủy bếp:\n{path}", "Hủy món", MessageBoxButton.OK, MessageBoxImage.Information);
    }

}
