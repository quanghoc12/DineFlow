using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;
using DineFlow.WPFApp.Services.Realtime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DineFlow.WPFApp.Features.Management.Tables;

public partial class TableManagementView : UserControl
{
    private readonly TableManagementViewModel _viewModel;
    private readonly ITableManagementService _service;
    private readonly StaffRealtimeClient _realtimeClient = new();
    private bool _realtimeStarted;

    public TableManagementView(TableManagementViewModel viewModel, ITableManagementService service)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _service = service;
        _realtimeClient.TableOtpChanged += async payload =>
        {
            Task updateTask = await Dispatcher.InvokeAsync(
                () => _viewModel.HandleTableOtpChangedAsync(payload),
                DispatcherPriority.Background);
            await updateTask;
        };
        DataContext = viewModel;
    }

    public async Task LoadAsync()
    {
        await EnsureRealtimeStartedAsync();
        await _viewModel.LoadAsync();
    }

    private async Task EnsureRealtimeStartedAsync()
    {
        if (_realtimeStarted)
        {
            return;
        }

        try
        {
            await _realtimeClient.StartAsync();
            _realtimeStarted = true;
        }
        catch
        {
            // Loading tables should still work even if realtime is temporarily unavailable.
        }
    }

    private void AreaTab_Click(object sender, RoutedEventArgs e)
    {
        AreaPanel.Visibility = Visibility.Visible;
        TablePanel.Visibility = Visibility.Collapsed;
        AreaTabButton.Tag = "Active";
        TableTabButton.Tag = null;
    }

    private void TableTab_Click(object sender, RoutedEventArgs e)
    {
        AreaPanel.Visibility = Visibility.Collapsed;
        TablePanel.Visibility = Visibility.Visible;
        AreaTabButton.Tag = null;
        TableTabButton.Tag = "Active";
    }

    private async void AddArea_Click(object sender, RoutedEventArgs e)
    {
        AreaEditorWindow dialog = new() { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await _viewModel.SaveAreaAsync(null, dialog.AreaNameValue, dialog.DisplayOrderValue);
    }

    private async void EditArea_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedAreaDto area) return;
        AreaEditorWindow dialog = new(area) { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true)
            await _viewModel.SaveAreaAsync(area, dialog.AreaNameValue, dialog.DisplayOrderValue);
    }

    private async void ToggleArea_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedAreaDto area) return;
        await _viewModel.ToggleAreaActiveAsync(area);
    }

    private async void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        TableEditorWindow dialog = new(_service, _viewModel.ManagedAreas, canResetOtp: _viewModel.CanResetOtp)
            { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.AreaValue is { } area)
            await _viewModel.CreateAsync(dialog.TableNameValue, area, dialog.DisplayOrderValue);
    }

    private async void EditTableRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedTableDto table) return;
        TableEditorWindow dialog = new(_service, _viewModel.ManagedAreas, table, _viewModel.CanResetOtp)
            { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.AreaValue is { } area)
            await _viewModel.UpdateAsync(table, dialog.TableNameValue, area, dialog.DisplayOrderValue);
        else
            await _viewModel.LoadAsync();
    }

    private void PreviewQrRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedTableDto table) return;
        new QrPreviewWindow(table) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private async void ResetOtpRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedTableDto table) return;
        if (MessageBox.Show(
                "Reset OTP sẽ tạo mã mới cho khách mới vào bàn.\nKhách đã xác thực trong session hiện tại vẫn tiếp tục gọi món được.",
                "Xác nhận reset OTP",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        await _viewModel.ResetOtpAsync(table);
    }
}
