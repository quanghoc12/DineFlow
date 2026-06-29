using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;
using System.Windows;

namespace DineFlow.WPFApp.Features.Management.Tables;

public partial class TableEditorWindow : Window
{
    private readonly ITableManagementService _service;
    private ManagedTableDto? _table;

    public TableEditorWindow(
        ITableManagementService service,
        IEnumerable<ManagedAreaDto> areas,
        ManagedTableDto? table = null)
    {
        InitializeComponent();
        _service = service;
        _table = table;
        AreaComboBox.ItemsSource = areas.Where(area => area.IsActive || area.AreaId == table?.AreaId).ToList();
        AreaComboBox.SelectedItem = AreaComboBox.Items.Cast<ManagedAreaDto>()
            .FirstOrDefault(area => area.AreaId == table?.AreaId) ?? AreaComboBox.Items.Cast<ManagedAreaDto>().FirstOrDefault();
        if (table is null) return;

        HeadingText.Text = "Chỉnh sửa bàn";
        CreateHint.Visibility = Visibility.Collapsed;
        TableNameTextBox.Text = table.TableName;
        OrderTextBox.Text = table.DisplayOrder.ToString();
        QrActions.Visibility = Visibility.Visible;
        RefreshTableState();
    }

    public string TableNameValue => TableNameTextBox.Text;
    public ManagedAreaDto? AreaValue => AreaComboBox.SelectedItem as ManagedAreaDto;
    public int DisplayOrderValue { get; private set; }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TableNameValue) || AreaValue is null)
        {
            ErrorText.Text = "Tên bàn và khu vực không được để trống.";
            return;
        }
        if (!int.TryParse(OrderTextBox.Text, out int order) || order < 0)
        {
            ErrorText.Text = "Thứ tự phải là số không âm.";
            return;
        }
        DisplayOrderValue = order;
        DialogResult = true;
    }

    private void PreviewQr_Click(object sender, RoutedEventArgs e)
    {
        if (_table is not null)
            new QrPreviewWindow(_table) { Owner = this }.ShowDialog();
    }

    private async void ResetQr_Click(object sender, RoutedEventArgs e)
    {
        if (_table is null || MessageBox.Show(
            "QR cũ sẽ mất hiệu lực. Tạo lại QR?", "Xác nhận",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        await RunAsync(async () => _table = await _service.ResetQrAsync(_table.TableId));
    }

    private async void ToggleActive_Click(object sender, RoutedEventArgs e)
    {
        if (_table is null) return;
        await RunAsync(async () =>
        {
            await _service.SetActiveAsync(_table.TableId, !_table.IsActive);
            _table.IsActive = !_table.IsActive;
        });
    }

    private void CopyUrl_Click(object sender, RoutedEventArgs e)
    {
        if (_table is null) return;
        Clipboard.SetText(_table.QrUrl);
        ErrorText.Text = "Đã copy URL.";
    }

    private async Task RunAsync(Func<Task> action)
    {
        try
        {
            ErrorText.Text = string.Empty;
            await action();
            RefreshTableState();
        }
        catch (Exception exception)
        {
            ErrorText.Text = exception.Message;
        }
    }

    private void RefreshTableState()
    {
        if (_table is null) return;
        UrlTextBox.Text = _table.QrUrl;
        ToggleButton.Content = _table.IsActive ? "Khóa bàn" : "Mở bàn";
    }
}
