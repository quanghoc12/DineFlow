using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;
using DineFlow.WPFApp.Core;
using System.Collections.ObjectModel;

namespace DineFlow.WPFApp.ViewModels;

public sealed class TableManagementViewModel : BaseViewModel
{
    private readonly ITableManagementService _service;
    private List<ManagedTableDto> _allTables = [];
    private ManagedTableDto? _selectedTable;
    private string _searchText = string.Empty;
    private string _areaFilter = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public TableManagementViewModel(ITableManagementService service)
    {
        _service = service;
    }

    public ObservableCollection<ManagedTableDto> Tables { get; } = [];
    public ObservableCollection<string> Areas { get; } = [];
    public ObservableCollection<ManagedAreaDto> ManagedAreas { get; } = [];

    public ManagedTableDto? SelectedTable
    {
        get => _selectedTable;
        set => SetProperty(ref _selectedTable, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    public string AreaFilter
    {
        get => _areaFilter;
        set
        {
            if (SetProperty(ref _areaFilter, value))
            {
                ApplyFilter();
            }
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
        _allTables = (await _service.GetAllAsync()).ToList();
        ManagedAreas.Clear();
        foreach (ManagedAreaDto area in await _service.GetAreasAsync())
        {
            ManagedAreas.Add(area);
        }
        string selectedArea = AreaFilter;
        Areas.Clear();
        Areas.Add(string.Empty);
        foreach (string area in _allTables.Select(table => table.Area).Distinct().Order())
        {
            Areas.Add(area);
        }
        AreaFilter = Areas.Contains(selectedArea) ? selectedArea : string.Empty;
        ApplyFilter();
    });

    public Task CreateAsync(string tableName, ManagedAreaDto area, int displayOrder) =>
        ExecuteAndReloadAsync(() => _service.CreateAsync(new CreateManagedTableRequest
        {
            TableName = tableName,
            AreaId = area.AreaId,
            Area = area.AreaName,
            DisplayOrder = displayOrder
        }));

    public Task UpdateAsync(ManagedTableDto table, string tableName, ManagedAreaDto area, int displayOrder) =>
        ExecuteAndReloadAsync(() => _service.UpdateAsync(new UpdateManagedTableRequest
        {
            TableId = table.TableId,
            TableName = tableName,
            AreaId = area.AreaId,
            Area = area.AreaName,
            DisplayOrder = displayOrder
        }));

    public Task SaveAreaAsync(ManagedAreaDto? area, string name, int displayOrder) =>
        ExecuteAndReloadAsync(() => _service.SaveAreaAsync(new SaveAreaRequest
        {
            AreaId = area?.AreaId,
            AreaName = name,
            DisplayOrder = displayOrder
        }));

    public Task ToggleAreaActiveAsync(ManagedAreaDto area) =>
        ExecuteAndReloadAsync(() => _service.SetAreaActiveAsync(area.AreaId, !area.IsActive));

    public Task ToggleActiveAsync(ManagedTableDto table) =>
        ExecuteAndReloadAsync(() => _service.SetActiveAsync(table.TableId, !table.IsActive));

    public Task ResetQrAsync(ManagedTableDto table) =>
        ExecuteAndReloadAsync(() => _service.ResetQrAsync(table.TableId));

    private async Task ExecuteAndReloadAsync(Func<Task> action)
    {
        await ExecuteAsync(action);
        if (string.IsNullOrEmpty(ErrorMessage))
        {
            await LoadAsync();
        }
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<ManagedTableDto> filtered = _allTables;
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(table =>
                table.TableName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                table.Area.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(AreaFilter))
        {
            filtered = filtered.Where(table =>
                table.Area.Equals(AreaFilter, StringComparison.OrdinalIgnoreCase));
        }

        Tables.Clear();
        foreach (ManagedTableDto table in filtered)
        {
            Tables.Add(table);
        }
    }
}
