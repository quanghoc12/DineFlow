using DineFlow.BusinessObjects.Tables;
using DineFlow.WPFApp.ViewModels;
using DineFlow.Services.Tables;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Management.Tables;

public partial class TableManagementView : UserControl
{
    private readonly TableManagementViewModel _viewModel;
    private readonly ITableManagementService _service;

    public TableManagementView(TableManagementViewModel viewModel, ITableManagementService service)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _service = service;
        DataContext = viewModel;
    }

    public Task LoadAsync() => _viewModel.LoadAsync();

    private async void AreaButton_Click(object sender, RoutedEventArgs e)
    {
        new AreaManagementWindow(_service) { Owner = Window.GetWindow(this) }.ShowDialog();
        await _viewModel.LoadAsync();
    }

    private async void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        TableEditorWindow dialog = new();
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.CreateAsync(dialog.TableNameValue, dialog.AreaValue);
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out ManagedTableDto table)) return;
        TableEditorWindow dialog = new(table);
        if (dialog.ShowDialog() == true)
        {
            await _viewModel.UpdateAsync(table, dialog.TableNameValue, dialog.AreaValue);
        }
    }

    private async void ToggleActiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out ManagedTableDto table)) return;
        string action = table.IsActive ? "khóa" : "mở lại";
        if (MessageBox.Show(
                $"Bạn có chắc muốn {action} bàn {table.TableName}?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            await _viewModel.ToggleActiveAsync(table);
        }
    }

    private async void ResetQrButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out ManagedTableDto table)) return;
        if (MessageBox.Show(
                "QR cũ sẽ mất hiệu lực ngay lập tức. Bạn có chắc muốn tạo lại?",
                "Tạo lại QR",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            await _viewModel.ResetQrAsync(table);
        }
    }

    private void PreviewQrButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out ManagedTableDto table)) return;
        new QrPreviewWindow(table) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void CopyUrlButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetSelection(out ManagedTableDto table)) return;
        Clipboard.SetText(table.QrUrl);
    }

    private bool TryGetSelection(out ManagedTableDto table)
    {
        if (_viewModel.SelectedTable is { } selected)
        {
            table = selected;
            return true;
        }
        table = null!;
        MessageBox.Show("Vui lòng chọn một bàn.", "Quản lý bàn");
        return false;
    }
}
