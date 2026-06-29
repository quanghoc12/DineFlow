using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace DineFlow.WPFApp.Features.Management.Menu;

public partial class ChoicePricingWindow : Window
{
    private readonly IMenuManagementService _service;
    private readonly ManagedMenuItemDto _menuItem;

    public ChoicePricingWindow(IMenuManagementService service, ManagedMenuItemDto menuItem)
    {
        InitializeComponent();
        _service = service;
        _menuItem = menuItem;
        ItemTitle.Text = $"Cấu hình: {menuItem.Name}";
        Loaded += async (_, _) => await ReloadAsync();
    }

    private ManagedChoiceGroupDto? SelectedGroup => GroupList.SelectedItem as ManagedChoiceGroupDto;
    private ManagedChoiceItemDto? SelectedChoice => ChoiceList.SelectedItem as ManagedChoiceItemDto;

    private async Task ReloadAsync()
    {
        try
        {
            IReadOnlyList<ManagedChoiceGroupDto> groups = await _service.GetChoiceGroupsAsync();
            GroupList.ItemsSource = groups;
            AssignGroupBox.ItemsSource = groups.Where(group => group.IsAvailable);
            ChannelBox.ItemsSource = (await _service.GetSalesChannelsAsync()).Where(channel => channel.IsActive);
            StatusText.Text = string.Empty;
        }
        catch (Exception exception) { StatusText.Text = exception.Message; }
    }

    private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedGroup is not { } group) return;
        GroupNameBox.Text = group.GroupName;
        RequiredBox.IsChecked = group.IsRequired;
        MaxSelectBox.Text = group.MaxSelectDefault.ToString(CultureInfo.InvariantCulture);
        ChoiceList.ItemsSource = group.Items;
    }

    private void ChoiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedChoice is not { } choice) return;
        ChoiceNameBox.Text = choice.ChoiceName;
        ExtraPriceBox.Text = choice.ExtraPrice.ToString(CultureInfo.InvariantCulture);
    }

    private void NewGroup_Click(object sender, RoutedEventArgs e)
    {
        GroupList.SelectedItem = null;
        GroupNameBox.Clear();
        RequiredBox.IsChecked = false;
        MaxSelectBox.Text = "1";
        ChoiceList.ItemsSource = null;
    }

    private async void SaveGroup_Click(object sender, RoutedEventArgs e) => await RunAsync(async () =>
    {
        if (!int.TryParse(MaxSelectBox.Text, out int max)) throw new InvalidOperationException("MaxSelect không hợp lệ.");
        await _service.SaveChoiceGroupAsync(new SaveChoiceGroupRequest
        {
            ChoiceGroupId = SelectedGroup?.ChoiceGroupId,
            GroupName = GroupNameBox.Text,
            IsRequired = RequiredBox.IsChecked == true,
            MaxSelectDefault = max
        });
        await ReloadAsync();
    });

    private async void ToggleGroup_Click(object sender, RoutedEventArgs e) => await RunAsync(async () =>
    {
        ManagedChoiceGroupDto group = SelectedGroup ?? throw new InvalidOperationException("Vui lòng chọn nhóm.");
        await _service.SetChoiceGroupAvailabilityAsync(group.ChoiceGroupId, !group.IsAvailable);
        await ReloadAsync();
    });

    private void NewChoice_Click(object sender, RoutedEventArgs e)
    {
        ChoiceList.SelectedItem = null;
        ChoiceNameBox.Clear();
        ExtraPriceBox.Text = "0";
    }

    private async void SaveChoice_Click(object sender, RoutedEventArgs e) => await RunAsync(async () =>
    {
        ManagedChoiceGroupDto group = SelectedGroup ?? throw new InvalidOperationException("Vui lòng chọn nhóm.");
        if (!decimal.TryParse(ExtraPriceBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
            throw new InvalidOperationException("Giá cộng thêm không hợp lệ.");
        await _service.SaveChoiceItemAsync(new SaveChoiceItemRequest
        {
            ChoiceItemId = SelectedChoice?.ChoiceItemId,
            ChoiceGroupId = group.ChoiceGroupId,
            ChoiceName = ChoiceNameBox.Text,
            ExtraPrice = price
        });
        await ReloadAsync();
    });

    private async void ToggleChoice_Click(object sender, RoutedEventArgs e) => await RunAsync(async () =>
    {
        ManagedChoiceItemDto choice = SelectedChoice ?? throw new InvalidOperationException("Vui lòng chọn lựa chọn.");
        await _service.SetChoiceItemAvailabilityAsync(choice.ChoiceItemId, !choice.IsAvailable);
        await ReloadAsync();
    });

    private async void Assign_Click(object sender, RoutedEventArgs e) => await RunAsync(async () =>
    {
        ManagedChoiceGroupDto group = AssignGroupBox.SelectedItem as ManagedChoiceGroupDto
            ?? throw new InvalidOperationException("Vui lòng chọn nhóm.");
        if (!int.TryParse(DisplayOrderBox.Text, out int order)) throw new InvalidOperationException("Thứ tự không hợp lệ.");
        int? max = string.IsNullOrWhiteSpace(OverrideMaxBox.Text)
            ? null
            : int.TryParse(OverrideMaxBox.Text, out int value) ? value : throw new InvalidOperationException("MaxSelect không hợp lệ.");
        await _service.AssignChoiceGroupAsync(new AssignChoiceGroupRequest
        {
            MenuItemId = _menuItem.MenuItemId,
            ChoiceGroupId = group.ChoiceGroupId,
            DisplayOrder = order,
            MaxSelect = max
        });
    });

    private async void Unassign_Click(object sender, RoutedEventArgs e) => await RunAsync(async () =>
    {
        ManagedChoiceGroupDto group = AssignGroupBox.SelectedItem as ManagedChoiceGroupDto
            ?? throw new InvalidOperationException("Vui lòng chọn nhóm.");
        await _service.RemoveChoiceGroupAssignmentAsync(_menuItem.MenuItemId, group.ChoiceGroupId);
    });

    private async void SaveChannelPrice_Click(object sender, RoutedEventArgs e) => await RunAsync(async () =>
    {
        ManagedSalesChannelDto channel = ChannelBox.SelectedItem as ManagedSalesChannelDto
            ?? throw new InvalidOperationException("Vui lòng chọn kênh bán.");
        if (!decimal.TryParse(ChannelExtraBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
            throw new InvalidOperationException("Giá theo kênh không hợp lệ.");
        await _service.SaveMenuItemChannelPriceAsync(new SaveChannelPriceRequest
        {
            MenuItemId = _menuItem.MenuItemId,
            SalesChannelId = channel.SalesChannelId,
            ChannelExtraPrice = price
        });
    });

    private async Task RunAsync(Func<Task> action)
    {
        try
        {
            StatusText.Text = string.Empty;
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
