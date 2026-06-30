using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using DineFlow.WPFApp.Features.Reports.ViewModels;

namespace DineFlow.WPFApp.Features.Reports.TopSelling;

public partial class TopSellingItemsReportView : UserControl
{
    private readonly TopSellingItemsReportViewModel _viewModel;

    public TopSellingItemsReportView()
        : this(new TopSellingItemsReportViewModel())
    {
    }

    public TopSellingItemsReportView(TopSellingItemsReportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += TopSellingItemsReportView_Loaded;
    }

    private async void TopSellingItemsReportView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= TopSellingItemsReportView_Loaded;
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
            $"top-selling-{_viewModel.FromDate:yyyyMMdd}-{_viewModel.ToDate:yyyyMMdd}-top{_viewModel.TopCount}.csv");
    }

    private async void ExportExcelButton_Click(object sender, RoutedEventArgs e)
    {
        await ExportAsync(
            _viewModel.ExportExcelAsync,
            "Excel file|*.xls",
            $"top-selling-{_viewModel.FromDate:yyyyMMdd}-{_viewModel.ToDate:yyyyMMdd}-top{_viewModel.TopCount}.xls");
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
