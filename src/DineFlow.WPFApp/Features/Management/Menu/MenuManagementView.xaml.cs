using DineFlow.BusinessObjects.Menu;
using DineFlow.WPFApp.ViewModels;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class MenuManagementView : UserControl
{
    private readonly MenuManagementViewModel _viewModel;
    private int? _editingCategoryId;

    public MenuManagementView(MenuManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    public async Task LoadAsync()
    {
        await _viewModel.LoadAsync();
        FillAllForms();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MenuManagementViewModel.SelectedCategory)) FillCategoryForm();
        if (e.PropertyName == nameof(MenuManagementViewModel.SelectedItem))
        {
            FillItemForm();
            FillChannelPriceForms();
        }
        if (e.PropertyName == nameof(MenuManagementViewModel.SelectedChoiceGroup))
        {
            FillChannelPriceForms();
        }
        if (e.PropertyName == nameof(MenuManagementViewModel.SelectedChoiceItem))
        {
            FillChannelPriceForms();
        }
        if (e.PropertyName == nameof(MenuManagementViewModel.SelectedSalesChannel)) FillChannelForm();
    }

    private void FillAllForms()
    {
        FillCategoryForm();
        FillItemForm();
        FillChannelForm();
        FillChannelPriceForms();
        BeginNewCategory();
    }

    private void FillCategoryForm()
    {
        ManagedCategoryDto? category = _viewModel.SelectedCategory;
        if (category is null || category.CategoryId == 0) return;
        BeginEditCategory(category);
    }

    private void FillItemForm()
    {
        ManagedMenuItemDto? item = _viewModel.SelectedItem;
        if (item is null) return;
        ItemNameBox.Text = item.Name;
        ItemCategoryBox.SelectedItem = _viewModel.Categories.FirstOrDefault(category => category.CategoryId == item.CategoryId);
        ItemPriceBox.Text = item.BasePrice.ToString(CultureInfo.InvariantCulture);
        ItemStockBox.Text = item.Stock?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        ItemImageBox.Text = item.ImageUrl ?? string.Empty;
        ItemDescriptionBox.Text = item.Description ?? string.Empty;
        ItemAvailableBox.IsChecked = item.IsAvailable;
    }

    private void FillChannelForm()
    {
        ManagedSalesChannelDto? channel = _viewModel.SelectedSalesChannel;
        if (channel is null) return;
        ChannelCodeBox.Text = channel.ChannelCode;
        ChannelNameBox.Text = channel.ChannelName;
        FillChannelPriceForms();
    }

    private void FillChannelPriceForms()
    {
        MenuChannelPriceBox.Text = _viewModel.GetMenuItemChannelExtraPrice(
            _viewModel.SelectedItem,
            _viewModel.SelectedSalesChannel).ToString(CultureInfo.InvariantCulture);
        ChoiceChannelPriceBox.Text = _viewModel.GetChoiceItemChannelExtraPrice(
            _viewModel.SelectedChoiceItem,
            _viewModel.SelectedSalesChannel).ToString(CultureInfo.InvariantCulture);
    }

    private void AddCategoryButton_Click(object sender, RoutedEventArgs e) => BeginNewCategory();

    private void EditCategoryRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is ManagedCategoryDto category)
        {
            BeginEditCategory(category);
        }
    }

    private void NewCategory_Click(object sender, RoutedEventArgs e)
    {
        BeginNewCategory();
    }

    private void BeginNewCategory()
    {
        _editingCategoryId = null;
        _viewModel.SelectedCategory = null;
        CategoryFormTitle.Text = "Thêm danh mục mới";
        CategoryFormHint.Text = "Thứ tự được gợi ý là cuối danh sách. Nếu nhập số đã tồn tại, các danh mục phía sau sẽ tự dịch xuống.";
        CategoryToggleButton.IsEnabled = false;
        CategoryNameBox.Clear();
        CategoryDescriptionBox.Clear();
        CategoryOrderBox.Text = GetNextCategoryOrder().ToString(CultureInfo.InvariantCulture);
    }

    private void BeginEditCategory(ManagedCategoryDto category)
    {
        _editingCategoryId = category.CategoryId;
        _viewModel.SelectedCategory = category;
        CategoryFormTitle.Text = $"Chỉnh sửa: {category.CategoryName}";
        CategoryFormHint.Text = "Khi đổi thứ tự, các danh mục nằm giữa vị trí cũ và mới sẽ tự dịch chuyển để giữ thứ tự duy nhất.";
        CategoryToggleButton.IsEnabled = true;
        CategoryNameBox.Text = category.CategoryName;
        CategoryDescriptionBox.Text = category.Description ?? string.Empty;
        CategoryOrderBox.Text = category.DisplayOrder.ToString(CultureInfo.InvariantCulture);
    }

    private int GetNextCategoryOrder()
    {
        return _viewModel.EditableCategories.Count == 0
            ? 0
            : _viewModel.EditableCategories.Max(category => category.DisplayOrder) + 1;
    }

    private async void SaveCategory_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseInt(CategoryOrderBox.Text, "Thứ tự danh mục", out int order)) return;
        await _viewModel.SaveCategoryAsync(new SaveCategoryRequest
        {
            CategoryId = _editingCategoryId,
            CategoryName = CategoryNameBox.Text,
            Description = CategoryDescriptionBox.Text,
            DisplayOrder = order
        });
        if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
        {
            BeginNewCategory();
        }
    }

    private async void ToggleCategory_Click(object sender, RoutedEventArgs e)
    {
        if (_editingCategoryId is not { } categoryId)
        {
            MessageBox.Show("Vui lòng bấm Sửa ở một dòng danh mục trước.", "Quản lý danh mục");
            return;
        }

        ManagedCategoryDto? category = _viewModel.EditableCategories.FirstOrDefault(x => x.CategoryId == categoryId);
        if (category is null)
        {
            MessageBox.Show("Không tìm thấy danh mục đang chỉnh sửa.", "Quản lý danh mục");
            return;
        }

        await _viewModel.ToggleCategoryAsync(category);
        BeginNewCategory();
    }

    private void NewItem_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectedItem = null;
        ItemNameBox.Clear();
        ItemCategoryBox.SelectedItem = _viewModel.Categories.FirstOrDefault(category => category.CategoryId > 0 && category.IsActive);
        ItemPriceBox.Text = "0";
        ItemStockBox.Clear();
        ItemImageBox.Clear();
        ItemDescriptionBox.Clear();
        ItemAvailableBox.IsChecked = true;
    }

    private async void SaveItem_Click(object sender, RoutedEventArgs e)
    {
        if (ItemCategoryBox.SelectedItem is not ManagedCategoryDto { CategoryId: > 0 } category)
        {
            MessageBox.Show("Vui lòng chọn danh mục hợp lệ.", "Quản lý thực đơn");
            return;
        }
        if (!TryParseDecimal(ItemPriceBox.Text, "Giá cơ bản", out decimal price)) return;
        int? stock = null;
        if (!string.IsNullOrWhiteSpace(ItemStockBox.Text))
        {
            if (!TryParseInt(ItemStockBox.Text, "Tồn kho", out int parsedStock)) return;
            stock = parsedStock;
        }

        await _viewModel.SaveItemAsync(new SaveMenuItemRequest
        {
            MenuItemId = _viewModel.SelectedItem?.MenuItemId,
            CategoryId = category.CategoryId,
            Name = ItemNameBox.Text,
            Description = ItemDescriptionBox.Text,
            BasePrice = price,
            ImageUrl = ItemImageBox.Text,
            Stock = stock,
            IsAvailable = ItemAvailableBox.IsChecked == true
        });
    }

    private async void ToggleItem_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedItem is not { } item)
        {
            MessageBox.Show("Vui lòng chọn món.", "Quản lý thực đơn");
            return;
        }
        await _viewModel.ToggleItemAsync(item);
    }

    private async void AssignGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedItem is not { } item)
        {
            MessageBox.Show("Vui lòng chọn món cần gán nhóm phụ.", "Quản lý thực đơn");
            return;
        }
        if (AssignGroupBox.SelectedItem is not ManagedChoiceGroupDto group)
        {
            MessageBox.Show("Vui lòng chọn nhóm phụ.", "Quản lý thực đơn");
            return;
        }
        if (!TryParseInt(AssignOrderBox.Text, "Thứ tự nhóm phụ", out int order)) return;
        int? maxSelect = null;
        if (!string.IsNullOrWhiteSpace(AssignMaxBox.Text))
        {
            if (!TryParseInt(AssignMaxBox.Text, "MaxSelect riêng", out int parsedMax)) return;
            maxSelect = parsedMax;
        }

        await _viewModel.AssignChoiceGroupAsync(new AssignChoiceGroupRequest
        {
            MenuItemId = item.MenuItemId,
            ChoiceGroupId = group.ChoiceGroupId,
            DisplayOrder = order,
            MaxSelect = maxSelect
        });
    }

    private async void RemoveAssignedGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedItem is not { } item ||
            _viewModel.SelectedAssignedChoiceGroup is not { } group)
        {
            MessageBox.Show("Vui lòng chọn món và nhóm phụ đã gán.", "Quản lý thực đơn");
            return;
        }
        await _viewModel.RemoveChoiceGroupAssignmentAsync(item, group);
    }

    private async void AddChoiceGroupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ChoiceGroupEditorWindow dialog = new(_viewModel.ChoiceGroups)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                await SaveChoiceGroupWithItemsAsync(dialog);
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Không thể mở nhóm phụ");
        }
    }

    private async void EditChoiceGroupRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is ManagedChoiceGroupDto group)
        {
            try
            {
                ChoiceGroupEditorWindow dialog = new(_viewModel.ChoiceGroups, group)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true)
                {
                    await SaveChoiceGroupWithItemsAsync(dialog);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Không thể mở nhóm phụ");
            }
        }
    }

    private async Task SaveChoiceGroupWithItemsAsync(ChoiceGroupEditorWindow dialog)
    {
        await _viewModel.SaveChoiceGroupAsync(dialog.Request);
        if (!string.IsNullOrEmpty(_viewModel.ErrorMessage)) return;

        ManagedChoiceGroupDto? savedGroup = dialog.Request.ChoiceGroupId is { } groupId
            ? _viewModel.ChoiceGroups.FirstOrDefault(group => group.ChoiceGroupId == groupId)
            : _viewModel.ChoiceGroups.FirstOrDefault(group =>
                group.GroupName.Equals(dialog.Request.GroupName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (savedGroup is null)
        {
            MessageBox.Show("Đã lưu nhóm nhưng không tìm thấy nhóm để lưu lựa chọn.", "Quản lý nhóm phụ");
            return;
        }

        foreach (SaveChoiceItemRequest choiceRequest in dialog.ChoiceRequests)
        {
            choiceRequest.ChoiceGroupId = savedGroup.ChoiceGroupId;
            await _viewModel.SaveChoiceItemAsync(choiceRequest);
            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage)) return;
        }
    }

    private async void ToggleChoiceGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedChoiceGroup is not { } group)
        {
            MessageBox.Show("Vui lòng chọn nhóm phụ.", "Quản lý nhóm phụ");
            return;
        }

        await _viewModel.ToggleChoiceGroupAsync(group);
    }

    private async void ToggleChoiceItem_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedChoiceItem is not { } item)
        {
            MessageBox.Show("Vui lòng chọn lựa chọn phụ.", "Quản lý nhóm phụ");
            return;
        }

        await _viewModel.ToggleChoiceItemAsync(item);
    }

    private void NewChannel_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SelectedSalesChannel = null;
        ChannelCodeBox.Clear();
        ChannelNameBox.Clear();
    }

    private async void SaveChannel_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveSalesChannelAsync(new SaveSalesChannelRequest
        {
            SalesChannelId = _viewModel.SelectedSalesChannel?.SalesChannelId,
            ChannelCode = ChannelCodeBox.Text,
            ChannelName = ChannelNameBox.Text
        });
    }

    private async void ToggleChannel_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedSalesChannel is not { } channel)
        {
            MessageBox.Show("Vui lòng chọn kênh bán.", "Quản lý kênh bán");
            return;
        }
        await _viewModel.ToggleSalesChannelAsync(channel);
    }

    private async void SaveMenuChannelPrice_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedItem is not { } item ||
            _viewModel.SelectedSalesChannel is not { } channel)
        {
            MessageBox.Show("Vui lòng chọn món và kênh bán.", "Giá theo kênh");
            return;
        }
        if (!TryParseDecimal(MenuChannelPriceBox.Text, "Giá món theo kênh", out decimal extra)) return;
        await _viewModel.SaveMenuItemChannelPriceAsync(new SaveChannelPriceRequest
        {
            MenuItemId = item.MenuItemId,
            SalesChannelId = channel.SalesChannelId,
            ChannelExtraPrice = extra
        });
    }

    private async void SaveChoiceChannelPrice_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedChoiceItem is not { } item ||
            _viewModel.SelectedSalesChannel is not { } channel)
        {
            MessageBox.Show("Vui lòng chọn lựa chọn phụ và kênh bán.", "Giá theo kênh");
            return;
        }
        if (!TryParseDecimal(ChoiceChannelPriceBox.Text, "Giá lựa chọn theo kênh", out decimal extra)) return;
        await _viewModel.SaveChoiceItemChannelPriceAsync(new SaveChannelPriceRequest
        {
            MenuItemId = 0,
            ChoiceItemId = item.ChoiceItemId,
            SalesChannelId = channel.SalesChannelId,
            ChannelExtraPrice = extra
        });
    }

    private static bool TryParseInt(string value, string fieldName, out int result)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)) return true;
        MessageBox.Show($"{fieldName} không hợp lệ.", "DineFlow");
        return false;
    }

    private static bool TryParseDecimal(string value, string fieldName, out decimal result)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result)) return true;
        MessageBox.Show($"{fieldName} không hợp lệ.", "DineFlow");
        return false;
    }
}
