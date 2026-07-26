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
        _pdfPrintService.PrintKitchenBillCancel(_selectedTable, bill, reason);
        ShowCustomMessageBox("Bếp đã nhận thông báo hủy bill.", "Hủy bill", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PrintTemporaryBillButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBill is null || _selectedBill.Lines.Count == 0)
        {
            ShowCustomMessageBox("Bill chưa có món để in tạm tính.", "In tạm tính", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _pdfPrintService.PrintTemporaryBill(_selectedTable, _selectedBill);
        ShowCustomMessageBox("Đã chuẩn bị bill tạm tính.", "In tạm tính", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PrintKitchenOrderPdfIfNeeded(IReadOnlyList<(BillLinePreview Line, int Quantity)> kitchenLines)
    {
        if (_selectedBill is null || kitchenLines.Count == 0)
        {
            return;
        }

        _pdfPrintService.PrintKitchenOrder(_selectedTable, _selectedBill, kitchenLines);
        ShowCustomMessageBox("Bếp đã nhận bill.", "Thông báo bếp", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PrintPaymentReceiptPdf(BillPreview bill, PaymentDialogResult payment, string paymentMethods)
    {
        _pdfPrintService.PrintPaymentReceipt(
            _selectedTable,
            bill,
            payment.Parts.Sum(x => x.Amount),
            paymentMethods);

        ShowCustomMessageBox("Đã xác nhận thanh toán.", "Thanh toán", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PrintKitchenCancelPdf(BillLinePreview line, int cancelQuantity, string reason)
    {
        if (_selectedBill is null || cancelQuantity <= 0)
        {
            return;
        }

        _pdfPrintService.PrintKitchenCancel(_selectedTable, _selectedBill, line, cancelQuantity, reason);
        ShowCustomMessageBox("Bếp đã nhận thông báo hủy món.", "Hủy món", MessageBoxButton.OK, MessageBoxImage.Information);
    }

}
