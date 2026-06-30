using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DineFlow.WPFApp.Features.Reports.ViewModels;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Reports.BillHistory;

public partial class PaidBillHistoryView : UserControl
{
    private readonly PaidBillHistoryViewModel _viewModel;

    public PaidBillHistoryView()
        : this(new PaidBillHistoryViewModel())
    {
    }

    public PaidBillHistoryView(PaidBillHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += PaidBillHistoryView_Loaded;
    }

    private async void PaidBillHistoryView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= PaidBillHistoryView_Loaded;
        await _viewModel.LoadAsync();
    }

    private async void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    private void OpenCorrectionWorkspaceButton_Click(object sender, RoutedEventArgs e)
    {
        DashboardWorkspaceState.OpenPaymentCorrection();
    }

    private void OpenPaymentCorrectionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: PaidBillHistoryViewModel.PaidBillHistoryRowViewModel row })
        {
            row.OpenPaymentCorrection();
        }
    }

    private async void ExportCsvButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportAsync(
            _viewModel.ExportCsvAsync,
            "CSV file|*.csv",
            $"paid-bill-history-{_viewModel.FromDate:yyyyMMdd}-{_viewModel.ToDate:yyyyMMdd}.csv");
    }

    private async void ExportExcelButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportAsync(
            _viewModel.ExportExcelAsync,
            "Excel file|*.xls",
            $"paid-bill-history-{_viewModel.FromDate:yyyyMMdd}-{_viewModel.ToDate:yyyyMMdd}.xls");
    }

    private static async Task ExportAsync(
        Func<Task<byte[]>> exportAction,
        string filter,
        string fileName)
    {
        try
        {
            byte[] bytes = await exportAction();
            SaveFileDialog dialog = new()
            {
                Filter = filter,
                FileName = fileName
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            await File.WriteAllBytesAsync(dialog.FileName, bytes);
            MessageBox.Show("Đã xuất file thành công.", "Xuất báo cáo");
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Xuất báo cáo", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
