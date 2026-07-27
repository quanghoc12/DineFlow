using DineFlow.BusinessObjects.Menu;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class MenuManagementView : UserControl
{
    private readonly MenuManagementViewModel _viewModel;
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
        if (e.PropertyName == nameof(MenuManagementViewModel.SelectedSalesChannel)) FillChannelForm();
    }

    private void FillAllForms()
    {
        FillChannelForm();
    }

    private void FillChannelForm()
    {
        if (SelectChannelNotice == null || DefaultChannelNotice == null || ChannelPricingTabs == null) return;

        ManagedSalesChannelDto? channel = _viewModel.SelectedSalesChannel;
        if (channel is null)
        {
            SelectChannelNotice.Visibility = Visibility.Visible;
            DefaultChannelNotice.Visibility = Visibility.Collapsed;
            ChannelPricingTabs.Visibility = Visibility.Collapsed;
            return;
        }

        bool isDefaultChannel = IsDefaultDineInChannel(channel);

        if (isDefaultChannel)
        {
            SelectChannelNotice.Visibility = Visibility.Collapsed;
            DefaultChannelNotice.Visibility = Visibility.Visible;
            ChannelPricingTabs.Visibility = Visibility.Collapsed;
        }
        else
        {
            SelectChannelNotice.Visibility = Visibility.Collapsed;
            DefaultChannelNotice.Visibility = Visibility.Collapsed;
            ChannelPricingTabs.Visibility = Visibility.Visible;
        }
    }

    private static bool IsDefaultDineInChannel(ManagedSalesChannelDto channel) =>
        channel.ChannelCode.Equals("DINE_IN", StringComparison.OrdinalIgnoreCase) ||
        channel.ChannelName.Contains("tại quán", StringComparison.OrdinalIgnoreCase);

    private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        CategoryEditorWindow dialog = new(_viewModel.EditableCategories)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            if (dialog.DeleteRequested) return; // Cannot delete a new category anyway
            await _viewModel.SaveCategoryAsync(dialog.Request);
        }
    }

    private async void EditCategoryRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedCategoryDto category) return;
        CategoryEditorWindow dialog = new(_viewModel.EditableCategories, category)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            if (dialog.DeleteRequested)
            {
                await _viewModel.DeleteCategoryAsync(category);
                return;
            }

            await _viewModel.SaveCategoryAsync(dialog.Request);

            if (dialog.ToggleActiveRequested && string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                await _viewModel.ToggleCategoryAsync(category);
            }
        }
    }

    private async void AddMenuItemButton_Click(object sender, RoutedEventArgs e)
    {
        MenuItemEditorWindow dialog = new(_viewModel.Categories, _viewModel.ChoiceGroups)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            await SaveMenuItemWithAssignmentsAsync(dialog);
        }
    }

    private async void EditMenuItemRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedMenuItemDto item) return;
        MenuItemEditorWindow dialog = new(_viewModel.Categories, _viewModel.ChoiceGroups, item)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            if (dialog.DeleteRequested)
            {
                await _viewModel.DeleteItemAsync(item);
                return;
            }

            await SaveMenuItemWithAssignmentsAsync(dialog);
        }
    }

    private async Task SaveMenuItemWithAssignmentsAsync(MenuItemEditorWindow dialog)
    {
        await _viewModel.SaveItemAsync(dialog.Request);
        if (!string.IsNullOrEmpty(_viewModel.ErrorMessage)) return;

        ManagedMenuItemDto? savedItem = dialog.Request.MenuItemId is { } itemId
            ? _viewModel.Items.FirstOrDefault(item => item.MenuItemId == itemId)
            : _viewModel.Items.FirstOrDefault(item =>
                item.Name.Equals(dialog.Request.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (savedItem is null)
        {
            MessageBox.Show("Đã lưu món nhưng không tìm thấy món để gán nhóm phụ.", "Quản lý thực đơn");
            return;
        }

        // Save any newly created choice groups (created inline from the item editor dialog)
        Dictionary<string, int> newGroupNameToId = [];
        foreach ((SaveChoiceGroupRequest groupRequest, List<SaveChoiceItemRequest> choiceRequests) in dialog.PendingNewGroups)
        {
            await _viewModel.SaveChoiceGroupAsync(groupRequest);
            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage)) return;

            // Find the newly saved group by name to get its real ID
            ManagedChoiceGroupDto? savedGroup = _viewModel.ChoiceGroups.FirstOrDefault(g =>
                g.GroupName.Equals(groupRequest.GroupName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (savedGroup is null) continue;

            newGroupNameToId[groupRequest.GroupName.Trim()] = savedGroup.ChoiceGroupId;

            foreach (SaveChoiceItemRequest choiceRequest in choiceRequests)
            {
                choiceRequest.ChoiceGroupId = savedGroup.ChoiceGroupId;
                await _viewModel.SaveChoiceItemAsync(choiceRequest);
                if (!string.IsNullOrEmpty(_viewModel.ErrorMessage)) return;
            }
        }

        foreach (int removedChoiceGroupId in dialog.RemovedChoiceGroupIds)
        {
            // Only remove if it's a real (positive) ID — negative IDs are pending groups never saved
            if (removedChoiceGroupId <= 0) continue;
            ManagedMenuItemChoiceGroupDto? assigned = savedItem.ChoiceGroups.FirstOrDefault(group => group.ChoiceGroupId == removedChoiceGroupId);
            if (assigned is not null)
            {
                await _viewModel.RemoveChoiceGroupAssignmentAsync(savedItem, assigned);
                if (!string.IsNullOrEmpty(_viewModel.ErrorMessage)) return;
            }
        }

        foreach (AssignChoiceGroupRequest assignment in dialog.AssignmentRequests)
        {
            assignment.MenuItemId = savedItem.MenuItemId;

            // If this assignment references a pending (negative) group ID, resolve to real ID by name
            if (assignment.ChoiceGroupId < 0)
            {
                ManagedChoiceGroupDto? tempGroup = dialog.PendingNewGroups
                    .Select(p => new ManagedChoiceGroupDto { GroupName = p.Group.GroupName })
                    .FirstOrDefault();

                // Look up by matching order in pendingNewGroups list (negative ID encoding: -(100 + index))
                int pendingIndex = -(assignment.ChoiceGroupId + 100) - 1;
                if (pendingIndex >= 0 && pendingIndex < dialog.PendingNewGroups.Count)
                {
                    string pendingName = dialog.PendingNewGroups[pendingIndex].Group.GroupName.Trim();
                    if (newGroupNameToId.TryGetValue(pendingName, out int realId))
                    {
                        assignment.ChoiceGroupId = realId;
                    }
                    else
                    {
                        continue; // Skip if we couldn't resolve — group save may have failed
                    }
                }
                else
                {
                    continue;
                }
            }

            await _viewModel.AssignChoiceGroupAsync(assignment);
            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage)) return;
        }
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

    private async void AddChannelButton_Click(object sender, RoutedEventArgs e)
    {
        SalesChannelEditorWindow dialog = new(_viewModel.SalesChannels)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            await _viewModel.SaveSalesChannelAsync(dialog.Request);
        }
    }

    private void SalesChannelRow_Click(object sender, MouseButtonEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is ManagedSalesChannelDto channel)
            _viewModel.SelectedSalesChannel = channel;
    }

    private async void EditChannelRow_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedSalesChannelDto channel) return;

        SalesChannelEditorWindow dialog = new(_viewModel.SalesChannels, channel)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            if (dialog.DeleteRequested)
            {
                var confirm = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa kênh bán hàng '{channel.ChannelName}' không?\nHành động này không thể hoàn tác.",
                    "Xác nhận xóa kênh",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.Yes)
                {
                    await _viewModel.DeleteSalesChannelAsync(channel);
                }
            }
            else if (dialog.ToggleActiveRequested)
            {
                await _viewModel.ToggleSalesChannelAsync(channel);
            }
            else
            {
                await _viewModel.SaveSalesChannelAsync(dialog.Request);
            }
        }
    }

    private async void EditMenuItemChannelPrice_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedMenuItemDto item ||
            _viewModel.SelectedSalesChannel is not { } channel)
        {
            return;
        }

        decimal currentExtra = _viewModel.GetMenuItemChannelExtraPrice(item, channel);

        MenuItemChannelPriceEditorWindow dialog = new(
            item.Name,
            item.BasePrice,
            currentExtra,
            channel.ChannelName)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            await _viewModel.SaveMenuItemChannelPriceAsync(new SaveChannelPriceRequest
            {
                MenuItemId = item.MenuItemId,
                SalesChannelId = channel.SalesChannelId,
                ChannelExtraPrice = dialog.ResultPrice
            });

            if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
            {
                MessageBox.Show(_viewModel.ErrorMessage, "Lỗi lưu giá");
            }
        }
    }

    private void EditChoiceGroupChannelPrices_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ManagedChoiceGroupDto choiceGroup ||
            _viewModel.SelectedSalesChannel is not { } channel)
        {
            return;
        }

        ChoiceGroupChannelPricesWindow dialog = new(choiceGroup, channel, _viewModel)
        {
            Owner = Window.GetWindow(this)
        };

        dialog.ShowDialog();
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

    private void CategoryTabButton_Click(object sender, RoutedEventArgs e)
    {
        CategoryPanel.Visibility = Visibility.Visible;
        MenuItemPanel.Visibility = Visibility.Collapsed;
        ChoiceGroupPanel.Visibility = Visibility.Collapsed;
        SalesChannelPanel.Visibility = Visibility.Collapsed;

        CategoryTabButton.Tag = "Active";
        MenuItemTabButton.Tag = null;
        ChoiceGroupTabButton.Tag = null;
        SalesChannelTabButton.Tag = null;
    }

    private void MenuItemTabButton_Click(object sender, RoutedEventArgs e)
    {
        CategoryPanel.Visibility = Visibility.Collapsed;
        MenuItemPanel.Visibility = Visibility.Visible;
        ChoiceGroupPanel.Visibility = Visibility.Collapsed;
        SalesChannelPanel.Visibility = Visibility.Collapsed;

        CategoryTabButton.Tag = null;
        MenuItemTabButton.Tag = "Active";
        ChoiceGroupTabButton.Tag = null;
        SalesChannelTabButton.Tag = null;
    }

    private void ChoiceGroupTabButton_Click(object sender, RoutedEventArgs e)
    {
        CategoryPanel.Visibility = Visibility.Collapsed;
        MenuItemPanel.Visibility = Visibility.Collapsed;
        ChoiceGroupPanel.Visibility = Visibility.Visible;
        SalesChannelPanel.Visibility = Visibility.Collapsed;

        CategoryTabButton.Tag = null;
        MenuItemTabButton.Tag = null;
        ChoiceGroupTabButton.Tag = "Active";
        SalesChannelTabButton.Tag = null;
    }

    private void SalesChannelTabButton_Click(object sender, RoutedEventArgs e)
    {
        CategoryPanel.Visibility = Visibility.Collapsed;
        MenuItemPanel.Visibility = Visibility.Collapsed;
        ChoiceGroupPanel.Visibility = Visibility.Collapsed;
        SalesChannelPanel.Visibility = Visibility.Visible;

        CategoryTabButton.Tag = null;
        MenuItemTabButton.Tag = null;
        ChoiceGroupTabButton.Tag = null;
        SalesChannelTabButton.Tag = "Active";
    }
}
