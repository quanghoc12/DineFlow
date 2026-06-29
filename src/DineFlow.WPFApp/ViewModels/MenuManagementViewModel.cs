using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;
using DineFlow.WPFApp.Core;
using System.Collections.ObjectModel;

namespace DineFlow.WPFApp.ViewModels;

public sealed class MenuManagementViewModel : BaseViewModel
{
    private readonly IMenuManagementService _service;
    private List<ManagedMenuItemDto> _allItems = [];
    private ManagedMenuItemDto? _selectedItem;
    private ManagedCategoryDto? _selectedCategory;
    private ManagedCategoryDto? _categoryFilter;
    private ManagedChoiceGroupDto? _selectedChoiceGroup;
    private ManagedChoiceItemDto? _selectedChoiceItem;
    private ManagedMenuItemChoiceGroupDto? _selectedAssignedChoiceGroup;
    private ManagedSalesChannelDto? _selectedSalesChannel;
    private string _searchText = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public MenuManagementViewModel(IMenuManagementService service) => _service = service;

    public ObservableCollection<ManagedCategoryDto> Categories { get; } = [];
    public ObservableCollection<ManagedMenuItemDto> Items { get; } = [];
    public ObservableCollection<ManagedChoiceGroupDto> ChoiceGroups { get; } = [];
    public ObservableCollection<ManagedChoiceItemDto> ChoiceItems { get; } = [];
    public ObservableCollection<ManagedMenuItemChoiceGroupDto> AssignedChoiceGroups { get; } = [];
    public ObservableCollection<ManagedSalesChannelDto> SalesChannels { get; } = [];

    public ManagedMenuItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                RefreshAssignedChoiceGroups();
            }
        }
    }

    public ManagedCategoryDto? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public ManagedCategoryDto? CategoryFilter
    {
        get => _categoryFilter;
        set
        {
            if (SetProperty(ref _categoryFilter, value)) ApplyFilter();
        }
    }

    public ManagedChoiceGroupDto? SelectedChoiceGroup
    {
        get => _selectedChoiceGroup;
        set
        {
            if (SetProperty(ref _selectedChoiceGroup, value))
            {
                RefreshChoiceItems();
            }
        }
    }

    public ManagedChoiceItemDto? SelectedChoiceItem
    {
        get => _selectedChoiceItem;
        set => SetProperty(ref _selectedChoiceItem, value);
    }

    public ManagedMenuItemChoiceGroupDto? SelectedAssignedChoiceGroup
    {
        get => _selectedAssignedChoiceGroup;
        set => SetProperty(ref _selectedAssignedChoiceGroup, value);
    }

    public ManagedSalesChannelDto? SelectedSalesChannel
    {
        get => _selectedSalesChannel;
        set => SetProperty(ref _selectedSalesChannel, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value)) ApplyFilter();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public Task LoadAsync() => ExecuteAsync(async () =>
    {
        int? selectedCategoryId = SelectedCategory?.CategoryId;
        int? selectedFilterId = CategoryFilter?.CategoryId;
        int? selectedItemId = SelectedItem?.MenuItemId;
        int? selectedGroupId = SelectedChoiceGroup?.ChoiceGroupId;
        int? selectedChannelId = SelectedSalesChannel?.SalesChannelId;

        IReadOnlyList<ManagedCategoryDto> categories = await _service.GetCategoriesAsync();
        _allItems = (await _service.GetItemsAsync()).ToList();
        IReadOnlyList<ManagedChoiceGroupDto> groups = await _service.GetChoiceGroupsAsync();
        IReadOnlyList<ManagedSalesChannelDto> channels = await _service.GetSalesChannelsAsync();

        Categories.Clear();
        Categories.Add(new ManagedCategoryDto { CategoryId = 0, CategoryName = "Tất cả danh mục", IsActive = true });
        foreach (ManagedCategoryDto category in categories) Categories.Add(category);

        ChoiceGroups.Clear();
        foreach (ManagedChoiceGroupDto group in groups) ChoiceGroups.Add(group);

        SalesChannels.Clear();
        foreach (ManagedSalesChannelDto channel in channels) SalesChannels.Add(channel);

        SelectedCategory = Categories.FirstOrDefault(category => category.CategoryId == selectedCategoryId);
        CategoryFilter = Categories.FirstOrDefault(category => category.CategoryId == selectedFilterId) ?? Categories[0];
        SelectedChoiceGroup = ChoiceGroups.FirstOrDefault(group => group.ChoiceGroupId == selectedGroupId);
        SelectedSalesChannel = SalesChannels.FirstOrDefault(channel => channel.SalesChannelId == selectedChannelId);

        ApplyFilter();
        SelectedItem = Items.FirstOrDefault(item => item.MenuItemId == selectedItemId) ?? SelectedItem;
        RefreshChoiceItems();
        RefreshAssignedChoiceGroups();
    });

    public Task SaveCategoryAsync(SaveCategoryRequest request) => ExecuteAndReloadAsync(() => _service.SaveCategoryAsync(request));
    public Task ToggleCategoryAsync(ManagedCategoryDto category) => ExecuteAndReloadAsync(() => _service.SetCategoryActiveAsync(category.CategoryId, !category.IsActive));
    public Task SaveItemAsync(SaveMenuItemRequest request) => ExecuteAndReloadAsync(() => _service.SaveItemAsync(request));
    public Task ToggleItemAsync(ManagedMenuItemDto item) => ExecuteAndReloadAsync(() => _service.SetItemAvailabilityAsync(item.MenuItemId, !item.IsAvailable));
    public Task SaveChoiceGroupAsync(SaveChoiceGroupRequest request) => ExecuteAndReloadAsync(() => _service.SaveChoiceGroupAsync(request));
    public Task ToggleChoiceGroupAsync(ManagedChoiceGroupDto group) => ExecuteAndReloadAsync(() => _service.SetChoiceGroupAvailabilityAsync(group.ChoiceGroupId, !group.IsAvailable));
    public Task SaveChoiceItemAsync(SaveChoiceItemRequest request) => ExecuteAndReloadAsync(() => _service.SaveChoiceItemAsync(request));
    public Task ToggleChoiceItemAsync(ManagedChoiceItemDto item) => ExecuteAndReloadAsync(() => _service.SetChoiceItemAvailabilityAsync(item.ChoiceItemId, !item.IsAvailable));
    public Task AssignChoiceGroupAsync(AssignChoiceGroupRequest request) => ExecuteAndReloadAsync(() => _service.AssignChoiceGroupAsync(request));
    public Task RemoveChoiceGroupAssignmentAsync(ManagedMenuItemDto item, ManagedMenuItemChoiceGroupDto group) =>
        ExecuteAndReloadAsync(() => _service.RemoveChoiceGroupAssignmentAsync(item.MenuItemId, group.ChoiceGroupId));
    public Task SaveSalesChannelAsync(SaveSalesChannelRequest request) => ExecuteAndReloadAsync(() => _service.SaveSalesChannelAsync(request));
    public Task ToggleSalesChannelAsync(ManagedSalesChannelDto channel) => ExecuteAndReloadAsync(() => _service.SetSalesChannelActiveAsync(channel.SalesChannelId, !channel.IsActive));
    public Task SaveMenuItemChannelPriceAsync(SaveChannelPriceRequest request) => ExecuteAndReloadAsync(() => _service.SaveMenuItemChannelPriceAsync(request));
    public Task SaveChoiceItemChannelPriceAsync(SaveChannelPriceRequest request) => ExecuteAndReloadAsync(() => _service.SaveChoiceItemChannelPriceAsync(request));

    public decimal GetMenuItemChannelExtraPrice(ManagedMenuItemDto? item, ManagedSalesChannelDto? channel)
    {
        if (item is null || channel is null) return 0m;
        return item.ChannelPrices.FirstOrDefault(price => price.SalesChannelId == channel.SalesChannelId)?.ChannelExtraPrice ?? 0m;
    }

    public decimal GetChoiceItemChannelExtraPrice(ManagedChoiceItemDto? item, ManagedSalesChannelDto? channel)
    {
        if (item is null || channel is null) return 0m;
        return item.ChannelPrices.FirstOrDefault(price => price.SalesChannelId == channel.SalesChannelId)?.ChannelExtraPrice ?? 0m;
    }

    private async Task ExecuteAndReloadAsync(Func<Task> action)
    {
        await ExecuteAsync(action);
        if (string.IsNullOrEmpty(ErrorMessage)) await LoadAsync();
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        try { await action(); }
        catch (Exception exception) { ErrorMessage = exception.Message; }
        finally { IsBusy = false; }
    }

    private void ApplyFilter()
    {
        IEnumerable<ManagedMenuItemDto> filtered = _allItems;
        if (CategoryFilter is { CategoryId: > 0 })
            filtered = filtered.Where(item => item.CategoryId == CategoryFilter.CategoryId);
        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(item =>
                item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                item.CategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Items.Clear();
        foreach (ManagedMenuItemDto item in filtered) Items.Add(item);
    }

    private void RefreshChoiceItems()
    {
        ChoiceItems.Clear();
        if (SelectedChoiceGroup is null) return;
        foreach (ManagedChoiceItemDto item in SelectedChoiceGroup.Items.OrderBy(item => item.ChoiceName))
        {
            ChoiceItems.Add(item);
        }
        SelectedChoiceItem = ChoiceItems.FirstOrDefault();
    }

    private void RefreshAssignedChoiceGroups()
    {
        AssignedChoiceGroups.Clear();
        if (SelectedItem is null) return;
        foreach (ManagedMenuItemChoiceGroupDto group in SelectedItem.ChoiceGroups.OrderBy(group => group.DisplayOrder))
        {
            AssignedChoiceGroups.Add(group);
        }
        SelectedAssignedChoiceGroup = AssignedChoiceGroups.FirstOrDefault();
    }
}
