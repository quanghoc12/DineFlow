using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Management.Tables;

public partial class AreaManagementWindow : Window
{
    private readonly ITableManagementService _service;
    private ManagedAreaDto? SelectedArea => AreaList.SelectedItem as ManagedAreaDto;

    public AreaManagementWindow(ITableManagementService service)
    {
        InitializeComponent();
        _service = service;
        Loaded += async (_, _) => await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        AreaList.ItemsSource = await _service.GetAreasAsync();
        StatusText.Text = string.Empty;
    }

    private void AreaList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedArea is not { } area) return;
        AreaNameBox.Text = area.AreaName;
        DisplayOrderBox.Text = area.DisplayOrder.ToString();
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        AreaList.SelectedItem = null;
        AreaNameBox.Clear();
        DisplayOrderBox.Text = "0";
    }

    private async void Save_Click(object sender, RoutedEventArgs e) => await RunAsync(async () =>
    {
        if (!int.TryParse(DisplayOrderBox.Text, out int order))
            throw new InvalidOperationException("Thứ tự hiển thị không hợp lệ.");
        await _service.SaveAreaAsync(new SaveAreaRequest
        {
            AreaId = SelectedArea?.AreaId,
            AreaName = AreaNameBox.Text,
            DisplayOrder = order
        });
        await ReloadAsync();
    });

    private async void Toggle_Click(object sender, RoutedEventArgs e) => await RunAsync(async () =>
    {
        ManagedAreaDto area = SelectedArea ?? throw new InvalidOperationException("Vui lòng chọn khu vực.");
        await _service.SetAreaActiveAsync(area.AreaId, !area.IsActive);
        await ReloadAsync();
    });

    private async Task RunAsync(Func<Task> action)
    {
        try
        {
            await action();
            StatusText.Foreground = System.Windows.Media.Brushes.SeaGreen;
            StatusText.Text = "Đã lưu.";
        }
        catch (Exception exception)
        {
            StatusText.Foreground = System.Windows.Media.Brushes.Firebrick;
            StatusText.Text = exception.Message;
        }
    }
}
