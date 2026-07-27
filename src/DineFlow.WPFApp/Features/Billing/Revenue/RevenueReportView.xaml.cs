using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Billing.Revenue;

public partial class RevenueReportView : UserControl
{
    private readonly RevenueReportViewModel _viewModel;

    public RevenueReportView()
        : this(new RevenueReportViewModel())
    {
    }

    public RevenueReportView(RevenueReportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    public async Task LoadTodayRevenueAsync()
    {
        _viewModel.SelectedDate = DateTime.Today;
        await _viewModel.LoadAsync();
    }

    private async void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    private void EditPaymentButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is BillHistoryRowViewModel row)
        {
            // Mở cửa sổ chỉnh sửa phương thức thanh toán
            var editWindow = new PaymentEditWindow(row);
            editWindow.Owner = Window.GetWindow(this);
            if (editWindow.ShowDialog() == true)
            {
                // Tải lại dữ liệu sau khi sửa đổi thành công
                _ = _viewModel.LoadAsync();
            }
        }
    }
}
