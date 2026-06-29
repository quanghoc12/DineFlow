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
    private ManagedCategoryDto? _categoryFilter;
    private string _searchText = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public MenuManagementViewModel(IMenuManagementService service) => _service = service;

    public ObservableCollection<ManagedCategoryDto> Categories { get; } = [];
    public ObservableCollection<ManagedMenuItemDto> Items { get; } = [];

    public ManagedMenuItemDto? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public ManagedCategoryDto? CategoryFilter
    {
        get => _categoryFilter;
        set
        {
            if (SetProperty(ref _categoryFilter, value)) ApplyFilter();
        }
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
        int? selectedCategoryId = CategoryFilter?.CategoryId;
        IReadOnlyList<ManagedCategoryDto> categories = await _service.GetCategoriesAsync();
        _allItems = (await _service.GetItemsAsync()).ToList();
        Categories.Clear();
        Categories.Add(new ManagedCategoryDto { CategoryId = 0, CategoryName = "Tất cả danh mục", IsActive = true });
        foreach (ManagedCategoryDto category in categories) Categories.Add(category);
        CategoryFilter = Categories.FirstOrDefault(category => category.CategoryId == selectedCategoryId) ?? Categories[0];
        ApplyFilter();
    });

    public Task SaveItemAsync(SaveMenuItemRequest request) => ExecuteAndReloadAsync(() => _service.SaveItemAsync(request));
    public Task ToggleItemAsync(ManagedMenuItemDto item) => ExecuteAndReloadAsync(() => _service.SetItemAvailabilityAsync(item.MenuItemId, !item.IsAvailable));
    public Task SaveCategoryAsync(SaveCategoryRequest request) => ExecuteAndReloadAsync(() => _service.SaveCategoryAsync(request));
    public Task ToggleCategoryAsync(ManagedCategoryDto category) => ExecuteAndReloadAsync(() => _service.SetCategoryActiveAsync(category.CategoryId, !category.IsActive));

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
}
