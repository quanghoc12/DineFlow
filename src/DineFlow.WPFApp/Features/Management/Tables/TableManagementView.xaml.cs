using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;
using DineFlow.WPFApp.ViewModels;
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
        TableEditorWindow dialog = new(_service, _viewModel.ManagedAreas)
            { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.AreaValue is { } area)
            await _viewModel.CreateAsync(dialog.TableNameValue, area, dialog.DisplayOrderValue);
    }

    private async void EditTableRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedTableDto table) return;
        TableEditorWindow dialog = new(_service, _viewModel.ManagedAreas, table)
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
}
