using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DineFlow.WPFApp.Features.Reports.ViewModels;

namespace DineFlow.WPFApp.Features.Reports.Payments;

public partial class RevenueByPaymentMethodView : UserControl
{
    private readonly RevenueByPaymentMethodViewModel _viewModel;

    public RevenueByPaymentMethodView()
        : this(new RevenueByPaymentMethodViewModel())
    {
    }

    public RevenueByPaymentMethodView(RevenueByPaymentMethodViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += RevenueByPaymentMethodView_Loaded;
    }

    private async void RevenueByPaymentMethodView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= RevenueByPaymentMethodView_Loaded;
        await _viewModel.LoadAsync();
    }

    private async void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    private async void ExportCsvButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportAsync(
            _viewModel.ExportCsvAsync,
            "CSV file|*.csv",
            $"payment-method-revenue-{_viewModel.FromDate:yyyyMMdd}-{_viewModel.ToDate:yyyyMMdd}.csv");
    }

    private async void ExportExcelButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportAsync(
            _viewModel.ExportExcelAsync,
            "Excel file|*.xls",
            $"payment-method-revenue-{_viewModel.FromDate:yyyyMMdd}-{_viewModel.ToDate:yyyyMMdd}.xls");
    }

    private async Task ExportAsync(
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
