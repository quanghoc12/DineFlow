using DineFlow.BusinessObjects.Menu;
using DineFlow.WPFApp.ViewModels;
using DineFlow.Services.Menu;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class MenuManagementView : UserControl
{
    private readonly MenuManagementViewModel _viewModel;
    private readonly IMenuManagementService _service;

    public MenuManagementView(MenuManagementViewModel viewModel, IMenuManagementService service)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _service = service;
        DataContext = viewModel;
    }

    private void ChoicePricingButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetItem(out ManagedMenuItemDto item)) return;
        new ChoicePricingWindow(_service, item) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    public Task LoadAsync() => _viewModel.LoadAsync();

    private async void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        MenuItemEditorWindow dialog = new(_viewModel.Categories.Where(category => category.CategoryId > 0 && category.IsActive));
        if (dialog.ShowDialog() == true) await _viewModel.SaveItemAsync(dialog.Request);
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetItem(out ManagedMenuItemDto item)) return;
        MenuItemEditorWindow dialog = new(
            _viewModel.Categories.Where(category => category.CategoryId > 0 && category.IsActive),
            item);
        if (dialog.ShowDialog() == true) await _viewModel.SaveItemAsync(dialog.Request);
    }

    private async void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetItem(out ManagedMenuItemDto item)) return;
        await _viewModel.ToggleItemAsync(item);
    }

    private async void CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        CategoryManagementWindow dialog = new(
            _viewModel.Categories.Where(category => category.CategoryId > 0),
            _viewModel);
        dialog.Owner = Window.GetWindow(this);
        dialog.ShowDialog();
        await _viewModel.LoadAsync();
    }

    private bool TryGetItem(out ManagedMenuItemDto item)
    {
        if (_viewModel.SelectedItem is { } selected)
        {
            item = selected;
            return true;
        }
        item = null!;
        MessageBox.Show("Vui lòng chọn một món.", "Quản lý thực đơn");
        return false;
    }
}
