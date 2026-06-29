using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DineFlow.Services.Bills;
using DineFlow.Services.Menu;
using DineFlow.Services.Orders;
using DineFlow.Services.Requests;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{
    public string PendingChoiceSummary => BuildPendingChoiceSummary();

    private static MenuItemCard MapMenuItem(
        MenuCatalogItemDto item,
        IReadOnlyDictionary<int, string> categoryNames)
    {
        string categoryName = categoryNames.TryGetValue(item.CategoryId, out string? value)
            ? value
            : item.CategoryId.ToString(CultureInfo.InvariantCulture);

        return new MenuItemCard(
            item.MenuItemId,
            item.Name,
            categoryName,
            item.FinalPrice,
            ColorFromId(item.MenuItemId),
            item.ChoiceGroups
                .OrderBy(x => x.DisplayOrder)
                .Select(MapChoiceGroup)
                .ToList(),
            item.ImageUrl,
            item.IsOutOfStock);
    }

    private static ChoiceGroupCard MapChoiceGroup(MenuItemChoiceGroupDto group)
    {
        return new ChoiceGroupCard(
            group.ChoiceGroupId,
            group.GroupName,
            group.IsRequired,
            group.EffectiveMaxSelect,
            group.DisplayOrder,
            group.ChoiceItems
                .Where(x => x.IsAvailable)
                .Select(x => new ChoiceOptionCard(x.ChoiceItemId, x.ChoiceName, x.FinalExtraPrice))
                .ToList());
    }

    private static string BuildChoiceSummary(IEnumerable<ChoiceGroupCard> groups)
    {
        List<string> selectedGroups = groups
            .Select(group =>
            {
                List<string> selectedOptions = group.Options
                    .Where(option => option.IsSelected)
                    .Select(option => option.Name)
                    .ToList();

                return selectedOptions.Count == 0
                    ? null
                    : $"{group.GroupName}: {string.Join(", ", selectedOptions)}";
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        return selectedGroups.Count == 0
            ? "Không có ghi chú/Món thêm"
            : string.Join("; ", selectedGroups);
    }

    private void CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        string category = button.CommandParameter?.ToString() ?? "All";
        _selectedCategory = category == "All" ? null : category;

        if (category == "All")
        {
            AllCategoryButton.Tag = "Active";
            foreach (FilterOption option in CategoryFilterOptions)
            {
                option.IsActive = false;
            }

            ApplyMenuFilters();
            return;
        }

        AllCategoryButton.Tag = null;
        foreach (FilterOption option in CategoryFilterOptions)
        {
            option.IsActive = string.Equals(option.Value, category, StringComparison.OrdinalIgnoreCase);
        }
        ApplyMenuFilters();
    }

    private async void MenuTile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: MenuItemCard item })
        {
            return;
        }

        if (item.IsOutOfStock)
        {
            ShowCustomMessageBox(
                "Món này đang hết hàng, vui lòng mở bán lại trong Quản lý món trước khi thêm vào đơn.",
                "Thêm món",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (!_usesApiData)
        {
            ShowCustomMessageBox(
                "Chưa kết nối được dữ liệu database. Vui lòng chạy API và tải lại màn Order trước khi thêm món.",
                "Thêm món",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        AddMenuItemDialogResult? result = ShowAddMenuItemDialog(item);
        if (result is null)
        {
            return;
        }

        await AddMenuItemToBillAsync(
            item,
            result.ChoiceSummary,
            result.UnitPrice,
            result.SelectedChoices,
            result.Quantity,
            result.Note);
    }

    private void ChoiceGroupTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { CommandParameter: ChoiceGroupCard group })
        {
            ActivateChoiceGroup(group);
        }
    }

    private void ChoiceOption_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { CommandParameter: ChoiceOptionCard option } ||
            _activeChoiceGroup is null)
        {
            return;
        }

        if (_activeChoiceGroup.IsRequired || _activeChoiceGroup.EffectiveMaxSelect == 1)
        {
            foreach (ChoiceOptionCard item in _activeChoiceGroup.Options)
            {
                item.IsSelected = item == option;
                item.NotifyChanged();
            }
        }
        else
        {
            if (!option.IsSelected &&
                _activeChoiceGroup.Options.Count(x => x.IsSelected) >= _activeChoiceGroup.EffectiveMaxSelect)
            {
                return;
            }

            option.IsSelected = !option.IsSelected;
            option.NotifyChanged();
        }

        RefreshChoiceBindings();
    }

    private async void ConfirmChoiceButton_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingChoiceItem is null)
        {
            return;
        }

        ChoiceGroupCard? invalidRequiredGroup = PendingChoiceGroups.FirstOrDefault(x =>
            x.IsRequired &&
            x.Options.Count(option => option.IsSelected) == 0);

        if (invalidRequiredGroup is not null)
        {
            ActivateChoiceGroup(invalidRequiredGroup);
            ChoiceGuideText.Text = $"Vui lòng chọn {invalidRequiredGroup.GroupName}.";
            return;
        }

        string choiceSummary = BuildPendingChoiceSummary();
        decimal unitPrice = _pendingChoiceItem.Price + PendingChoiceGroups
            .SelectMany(x => x.Options)
            .Where(x => x.IsSelected)
            .Sum(x => x.ExtraPrice);

        List<SelectedChoiceGroupRequest> selectedChoices = PendingChoiceGroups
            .Select(group => new SelectedChoiceGroupRequest
            {
                ChoiceGroupId = group.ChoiceGroupId,
                ChoiceItemIds = group.Options
                    .Where(option => option.IsSelected)
                    .Select(option => option.ChoiceItemId)
                    .Where(id => id > 0)
                    .ToList()
            })
            .Where(group => group.ChoiceItemIds.Count > 0)
            .ToList();

        await AddMenuItemToBillAsync(_pendingChoiceItem, choiceSummary, unitPrice, selectedChoices, 1, null);
        CloseChoiceSelection();
    }

    private void CancelChoiceButton_Click(object sender, RoutedEventArgs e)
    {
        CloseChoiceSelection();
    }

    private async Task AddMenuItemToBillAsync(
        MenuItemCard item,
        string choiceSummary,
        decimal unitPrice,
        IReadOnlyList<SelectedChoiceGroupRequest> selectedChoices,
        int quantity,
        string? note)
    {
        if (_usesApiData)
        {
            try
            {
                if (_isAddingMenuItem)
                {
                    return;
                }

                _isAddingMenuItem = true;
                TableCard table = EnsureSelectedTable();
                int? targetBillId = GetSelectedBillIdForCurrentTable();
                CreateOrderResponse response = await _apiClient.CreateStaffOrderAsync(new CreateStaffOrderRequest
                {
                    TableId = table.TableId,
                    TargetBillId = targetBillId,
                    SalesChannelCode = GetSelectedBillSalesChannelCode(),
                    Items =
                    [
                        new CreateOrderItemRequest
                        {
                            MenuItemId = item.MenuItemId,
                            Quantity = quantity,
                            Note = note,
                            SelectedChoices = selectedChoices.ToList()
                        }
                    ]
                });

                if (response.AcceptedItems.Count == 0)
                {
                    string reason = response.RejectedItems.FirstOrDefault()?.ReasonMessage ?? "Món không được nhận vào order.";
                    ShowCustomMessageBox(reason, "Thêm món", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await ReloadTableFromApiAsync(table.TableId, response.BillId ?? targetBillId);
                return;
            }
            catch (Exception ex)
            {
                ShowCustomMessageBox(GetFriendlyError(ex), "Thêm món", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            finally
            {
                _isAddingMenuItem = false;
            }
        }

        ShowCustomMessageBox(
            "Chưa kết nối được dữ liệu database. Không thể thêm món bằng dữ liệu tạm.",
            "Thêm món",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private string GetSelectedBillSalesChannelCode()
    {
        string? channelCode = _selectedBill?.SelectedChannelCode;
        return string.IsNullOrWhiteSpace(channelCode) ? "DINE_IN" : channelCode;
    }

    private async Task ReloadMenuCatalogForSelectedBillAsync()
    {
        if (!_usesApiData)
        {
            return;
        }

        string salesChannelCode = GetSelectedBillSalesChannelCode();
        if (string.Equals(_loadedMenuSalesChannelCode, salesChannelCode, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            MenuCatalogDto catalog = await _apiClient.GetMenuCatalogAsync(salesChannelCode);
            Dictionary<int, string> categoryNames = catalog.Categories
                .ToDictionary(x => x.CategoryId, x => x.CategoryName);
            Dictionary<int, int> categoryOrders = catalog.Categories
                .ToDictionary(x => x.CategoryId, x => x.DisplayOrder);

            List<MenuItemCard> loadedMenuItems = catalog.Items
                .OrderBy(x => categoryOrders.TryGetValue(x.CategoryId, out int categoryOrder) ? categoryOrder : int.MaxValue)
                .ThenBy(x => categoryNames.TryGetValue(x.CategoryId, out string? categoryName) ? categoryName : string.Empty)
                .ThenBy(x => x.Name)
                .Select(x => MapMenuItem(x, categoryNames))
                .ToList();

            _menuItems.Clear();
            _menuItems.AddRange(loadedMenuItems);
            _loadedMenuSalesChannelCode = salesChannelCode;
            RebuildCategoryFilters(catalog.Categories);
            ApplyMenuFilters();
        }
        catch
        {
            // Keep the previous catalog visible if channel-specific pricing cannot be loaded.
        }
    }

    private void OpenChoiceSelection(MenuItemCard item)
    {
        _pendingChoiceItem = item;
        ChoiceItemNameText.Text = item.Name;
        ChoiceGuideText.Text = "Chọn option theo từng tab bên dưới.";
        PendingChoiceGroups.Clear();

        foreach (ChoiceGroupCard group in item.ChoiceGroups.OrderBy(x => x.DisplayOrder))
        {
            ChoiceGroupCard clone = group.Clone();
            PendingChoiceGroups.Add(clone);
        }

        ActivateChoiceGroup(PendingChoiceGroups.First());
        MenuCatalogPanel.Visibility = Visibility.Collapsed;
        ChoiceSelectionPanel.Visibility = Visibility.Visible;
        RefreshChoiceBindings();
    }

    private void CloseChoiceSelection()
    {
        _pendingChoiceItem = null;
        _activeChoiceGroup = null;
        PendingChoiceGroups.Clear();
        ActiveChoiceOptions.Clear();
        ChoiceSelectionPanel.Visibility = Visibility.Collapsed;
        MenuCatalogPanel.Visibility = Visibility.Visible;
        OnPropertyChanged(nameof(PendingChoiceSummary));
    }

    private void ActivateChoiceGroup(ChoiceGroupCard group)
    {
        _activeChoiceGroup = group;

        foreach (ChoiceGroupCard item in PendingChoiceGroups)
        {
            item.IsActive = item == group;
            item.NotifyChanged();
        }

        ActiveChoiceOptions.Clear();
        foreach (ChoiceOptionCard option in group.Options)
        {
            ActiveChoiceOptions.Add(option);
        }
    }

    private void RefreshChoiceBindings()
    {
        OnPropertyChanged(nameof(PendingChoiceSummary));

        foreach (ChoiceGroupCard group in PendingChoiceGroups)
        {
            group.NotifyChanged();
        }
    }

    private void ApplyMenuFilters()
    {
        FilteredMenuItems.Clear();

        string keyword = SearchBox.Text.Trim();
        IEnumerable<MenuItemCard> items = _menuItems;

        if (!string.IsNullOrWhiteSpace(_selectedCategory))
        {
            items = items.Where(x => x.Category == _selectedCategory);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            items = items.Where(x => x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        foreach (MenuItemCard item in items)
        {
            FilteredMenuItems.Add(item);
        }

        OnPropertyChanged(nameof(MenuPageText));
    }

    private string BuildPendingChoiceSummary()
    {
        if (PendingChoiceGroups.Count == 0)
        {
            return "Chưa có lựa chọn";
        }

        List<string> selectedGroups = PendingChoiceGroups
            .Select(group =>
            {
                List<string> selectedOptions = group.Options
                    .Where(option => option.IsSelected)
                    .Select(option => option.Name)
                    .ToList();

                return selectedOptions.Count == 0
                    ? null
                    : $"{group.GroupName}: {string.Join(", ", selectedOptions)}";
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        return selectedGroups.Count == 0
            ? "Chưa có lựa chọn"
            : string.Join("; ", selectedGroups);
    }
}
