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
                .ToList());
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
            foreach (Button sibling in FindSiblingButtons(AllCategoryButton))
            {
                sibling.Tag = null;
            }

            ApplyMenuFilters();
            return;
        }

        AllCategoryButton.Tag = null;
        foreach (Button sibling in FindSiblingButtons(button))
        {
            sibling.Tag = null;
        }

        button.Tag = "Active";
        ApplyMenuFilters();
    }

    private async void MenuTile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: MenuItemCard item })
        {
            return;
        }

        if (!_usesApiData)
        {
            EnsureSelectedBill();
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
                    SalesChannelCode = "DINE_IN",
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

        BillPreview bill = EnsureSelectedBill();
        string lineDescription = BuildBillLineDescription(choiceSummary, note);
        BillLinePreview? existingLine = bill.Lines.FirstOrDefault(x =>
            x.MenuItemId == item.MenuItemId &&
            string.Equals(x.ChoiceSummary, lineDescription, StringComparison.Ordinal) &&
            x.UnitPrice == unitPrice);

        if (existingLine is null)
        {
            bill.Lines.Add(new BillLinePreview(
                0,
                item.MenuItemId,
                item.Name,
                lineDescription,
                quantity,
                0,
                unitPrice));
        }
        else
        {
            existingLine.Quantity += quantity;
            existingLine.NotifyChanged();
        }

        _selectedTable?.MarkServing();
        ApplyTableFilters();
        RefreshBill();
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

    private static List<MenuItemCard> CreateMockMenuItems()
    {
        List<ChoiceGroupCard> drinkGroups =
        [
            new ChoiceGroupCard("Size", true, 1, 1,
            [
                new ChoiceOptionCard("M", 0m),
                new ChoiceOptionCard("L", 7000m),
                new ChoiceOptionCard("XL", 12000m)
            ]),
            new ChoiceGroupCard("Đường", true, 1, 2,
            [
                new ChoiceOptionCard("0% đường", 0m),
                new ChoiceOptionCard("50% đường", 0m),
                new ChoiceOptionCard("100% đường", 0m)
            ]),
            new ChoiceGroupCard("Đá", true, 1, 3,
            [
                new ChoiceOptionCard("Không đá", 0m),
                new ChoiceOptionCard("Ít đá", 0m),
                new ChoiceOptionCard("Bình thường", 0m)
            ])
        ];

        List<ChoiceGroupCard> teaWithToppingGroups =
        [
            .. drinkGroups,
            new ChoiceGroupCard("Topping", false, 2, 4,
            [
                new ChoiceOptionCard("Trân châu đen", 7000m),
                new ChoiceOptionCard("Thạch cà phê", 6000m),
                new ChoiceOptionCard("Pudding trứng", 8000m)
            ])
        ];

        List<ChoiceGroupCard> milkTeaGroups =
        [
            .. drinkGroups,
            new ChoiceGroupCard("Topping", false, 3, 4,
            [
                new ChoiceOptionCard("Trân châu đen", 7000m),
                new ChoiceOptionCard("Thạch cà phê", 6000m),
                new ChoiceOptionCard("Pudding trứng", 8000m)
            ])
        ];

        List<ChoiceGroupCard> coffeeGroups =
        [
            new ChoiceGroupCard("Đường", true, 1, 1,
            [
                new ChoiceOptionCard("0% đường", 0m),
                new ChoiceOptionCard("50% đường", 0m),
                new ChoiceOptionCard("100% đường", 0m)
            ]),
            new ChoiceGroupCard("Đá", true, 1, 2,
            [
                new ChoiceOptionCard("Không đá", 0m),
                new ChoiceOptionCard("Ít đá", 0m),
                new ChoiceOptionCard("Bình thường", 0m)
            ])
        ];

        List<ChoiceGroupCard> spicyGroups =
        [
            new ChoiceGroupCard("Mức cay", true, 1, 1,
            [
                new ChoiceOptionCard("Không cay", 0m),
                new ChoiceOptionCard("Cay vừa", 0m),
                new ChoiceOptionCard("Rất cay", 0m)
            ])
        ];

        return
        [
            new MenuItemCard(1, "MILANO", "Do uong", 30000m, "#F59E0B"),
            new MenuItemCard(2, "APEROL SPRITZ", "Do uong", 30000m, "#F97316"),
            new MenuItemCard(3, "CUBA LIBRE", "Do uong", 30000m, "#7C2D12"),
            new MenuItemCard(4, "GIN FIZZ", "Do uong", 30000m, "#EAB308"),
            new MenuItemCard(5, "BLOODY MARY", "Do uong", 30000m, "#DC2626"),
            new MenuItemCard(6, "Cơm gà xối mỡ", "Mon chinh", 55000m, "#F97316"),
            new MenuItemCard(7, "Mì bò cay", "Mon chinh", 65000m, "#EF4444", spicyGroups),
            new MenuItemCard(8, "Bún thịt nướng", "Mon chinh", 50000m, "#84CC16"),
            new MenuItemCard(9, "Cơm sườn trứng", "Mon chinh", 60000m, "#A16207"),
            new MenuItemCard(10, "Trà đào", "Do uong", 30000m, "#FB923C", teaWithToppingGroups),
            new MenuItemCard(11, "Trà sữa truyền thống", "Do uong", 35000m, "#A16207", milkTeaGroups),
            new MenuItemCard(12, "Cà phê sữa", "Do uong", 28000m, "#78350F", coffeeGroups),
            new MenuItemCard(13, "Khoai tây chiên", "Mon them", 35000m, "#EAB308"),
            new MenuItemCard(14, "Salad nhỏ", "Mon them", 25000m, "#22C55E"),
            new MenuItemCard(15, "Xúc xích Đức nướng", "Mon them", 125000m, "#B45309")
        ];
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
