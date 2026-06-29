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
    private int? _editingChoiceGroupId;
    private int? _editingChoiceItemId;

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
            HideChoiceItemForm();
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
        HideChoiceGroupForm();
        HideChoiceItemForm();
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

    private void AddChoiceGroupButton_Click(object sender, RoutedEventArgs e)
    {
        BeginNewChoiceGroup();
    }

    private void EditChoiceGroupRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is ManagedChoiceGroupDto group)
        {
            BeginEditChoiceGroup(group);
        }
    }

    private void BeginNewChoiceGroup()
    {
        _editingChoiceGroupId = null;
        _viewModel.SelectedChoiceGroup = null;
        ChoiceGroupFormBorder.Visibility = Visibility.Visible;
        ChoiceGroupFormTitle.Text = "Thêm nhóm phụ";
        ChoiceGroupFormHint.Text = "Tạo nhóm bắt buộc cho size/đá hoặc nhóm tùy chọn cho topping/add-on.";
        ChoiceGroupToggleButton.IsEnabled = false;
        GroupNameBox.Clear();
        GroupRequiredBox.IsChecked = false;
        GroupMaxBox.Text = "1";
        GroupMaxBox.IsEnabled = true;
        GroupNameBox.Focus();
    }

    private void BeginEditChoiceGroup(ManagedChoiceGroupDto group)
    {
        _editingChoiceGroupId = group.ChoiceGroupId;
        _viewModel.SelectedChoiceGroup = group;
        ChoiceGroupFormBorder.Visibility = Visibility.Visible;
        ChoiceGroupFormTitle.Text = $"Chỉnh sửa nhóm: {group.GroupName}";
        ChoiceGroupFormHint.Text = "Bắt buộc = khách phải chọn đúng 1. Không bắt buộc = khách được chọn nhiều theo MaxSelect.";
        ChoiceGroupToggleButton.IsEnabled = true;
        GroupNameBox.Text = group.GroupName;
        GroupRequiredBox.IsChecked = group.IsRequired;
        GroupMaxBox.Text = group.MaxSelectDefault.ToString(CultureInfo.InvariantCulture);
        GroupMaxBox.IsEnabled = !group.IsRequired;
        GroupNameBox.Focus();
    }

    private void HideChoiceGroupForm()
    {
        _editingChoiceGroupId = null;
        ChoiceGroupFormBorder.Visibility = Visibility.Collapsed;
        GroupNameBox.Clear();
        GroupRequiredBox.IsChecked = false;
        GroupMaxBox.Text = "1";
        GroupMaxBox.IsEnabled = true;
    }

    private void CancelChoiceGroup_Click(object sender, RoutedEventArgs e)
    {
        HideChoiceGroupForm();
    }

    private async void SaveChoiceGroup_Click(object sender, RoutedEventArgs e)
    {
        int max = GroupRequiredBox.IsChecked == true ? 1 : 0;
        if (GroupRequiredBox.IsChecked != true &&
            !TryParseInt(GroupMaxBox.Text, "MaxSelect mặc định", out max)) return;

        await _viewModel.SaveChoiceGroupAsync(new SaveChoiceGroupRequest
        {
            ChoiceGroupId = _editingChoiceGroupId,
            GroupName = GroupNameBox.Text,
            IsRequired = GroupRequiredBox.IsChecked == true,
            MaxSelectDefault = max
        });
        if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
        {
            HideChoiceGroupForm();
        }
    }

    private async void ToggleChoiceGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_editingChoiceGroupId is not { } groupId)
        {
            MessageBox.Show("Vui lòng bấm Sửa ở một dòng nhóm phụ trước.", "Quản lý nhóm phụ");
            return;
        }
        ManagedChoiceGroupDto? group = _viewModel.ChoiceGroups.FirstOrDefault(x => x.ChoiceGroupId == groupId);
        if (group is null)
        {
            MessageBox.Show("Không tìm thấy nhóm phụ đang chỉnh sửa.", "Quản lý nhóm phụ");
            return;
        }

        await _viewModel.ToggleChoiceGroupAsync(group);
        HideChoiceGroupForm();
    }

    private void AddChoiceItemButton_Click(object sender, RoutedEventArgs e)
    {
        BeginNewChoiceItem();
    }

    private void EditChoiceItemRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is ManagedChoiceItemDto item)
        {
            BeginEditChoiceItem(item);
        }
    }

    private void BeginNewChoiceItem()
    {
        if (_viewModel.SelectedChoiceGroup is null)
        {
            MessageBox.Show("Vui lòng chọn nhóm phụ trước khi thêm lựa chọn.", "Quản lý nhóm phụ");
            return;
        }

        _editingChoiceItemId = null;
        _viewModel.SelectedChoiceItem = null;
        ChoiceItemFormBorder.Visibility = Visibility.Visible;
        ChoiceItemFormTitle.Text = $"Thêm lựa chọn cho nhóm: {_viewModel.SelectedChoiceGroup.GroupName}";
        ChoiceItemFormHint.Text = "Ví dụ: Ít đá, Size L, Trân châu. Giá cộng thêm nhập 0 nếu miễn phí.";
        ChoiceItemToggleButton.IsEnabled = false;
        ChoiceNameBox.Clear();
        ChoiceExtraBox.Text = "0";
        ChoiceNameBox.Focus();
    }

    private void BeginEditChoiceItem(ManagedChoiceItemDto item)
    {
        _editingChoiceItemId = item.ChoiceItemId;
        _viewModel.SelectedChoiceItem = item;
        ChoiceItemFormBorder.Visibility = Visibility.Visible;
        ChoiceItemFormTitle.Text = $"Chỉnh sửa lựa chọn: {item.ChoiceName}";
        ChoiceItemFormHint.Text = "Cập nhật tên hoặc giá cộng thêm của lựa chọn trong nhóm đang chọn.";
        ChoiceItemToggleButton.IsEnabled = true;
        ChoiceNameBox.Text = item.ChoiceName;
        ChoiceExtraBox.Text = item.ExtraPrice.ToString(CultureInfo.InvariantCulture);
        ChoiceNameBox.Focus();
    }

    private void HideChoiceItemForm()
    {
        _editingChoiceItemId = null;
        ChoiceItemFormBorder.Visibility = Visibility.Collapsed;
        ChoiceNameBox.Clear();
        ChoiceExtraBox.Text = "0";
    }

    private void CancelChoiceItem_Click(object sender, RoutedEventArgs e)
    {
        HideChoiceItemForm();
    }

    private async void SaveChoiceItem_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedChoiceGroup is not { } group)
        {
            MessageBox.Show("Vui lòng chọn nhóm phụ trước.", "Quản lý nhóm phụ");
            return;
        }
        if (!TryParseDecimal(ChoiceExtraBox.Text, "Giá cộng thêm", out decimal extra)) return;
        await _viewModel.SaveChoiceItemAsync(new SaveChoiceItemRequest
        {
            ChoiceItemId = _editingChoiceItemId,
            ChoiceGroupId = group.ChoiceGroupId,
            ChoiceName = ChoiceNameBox.Text,
            ExtraPrice = extra
        });
        if (string.IsNullOrEmpty(_viewModel.ErrorMessage))
        {
            HideChoiceItemForm();
        }
    }

    private async void ToggleChoiceItem_Click(object sender, RoutedEventArgs e)
    {
        if (_editingChoiceItemId is not { } itemId)
        {
            MessageBox.Show("Vui lòng bấm Sửa ở một dòng lựa chọn phụ trước.", "Quản lý nhóm phụ");
            return;
        }
        ManagedChoiceItemDto? item = _viewModel.ChoiceItems.FirstOrDefault(x => x.ChoiceItemId == itemId);
        if (item is null)
        {
            MessageBox.Show("Không tìm thấy lựa chọn phụ đang chỉnh sửa.", "Quản lý nhóm phụ");
            return;
        }

        await _viewModel.ToggleChoiceItemAsync(item);
        HideChoiceItemForm();
    }

    private void GroupRequiredBox_Changed(object sender, RoutedEventArgs e)
    {
        if (GroupRequiredBox.IsChecked == true)
        {
            GroupMaxBox.Text = "1";
            GroupMaxBox.IsEnabled = false;
        }
        else
        {
            GroupMaxBox.IsEnabled = true;
            if (string.IsNullOrWhiteSpace(GroupMaxBox.Text)) GroupMaxBox.Text = "1";
        }
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
