using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Bills;
using DineFlow.Services.Menu;
using DineFlow.Services.Orders;
using DineFlow.Services.Requests;
using DineFlow.WPFApp.Services;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView : UserControl, INotifyPropertyChanged, IAsyncDisposable
{
    private readonly StaffOrderApiClient _apiClient;
    private readonly StaffRealtimeClient _realtimeClient;
    private readonly PdfDemoPrintService _pdfPrintService;
    private readonly IMenuManagementService _menuManagementService;
    private readonly IBillService _billService;
    private readonly SemaphoreSlim _billReloadLock = new(1, 1);
    private readonly SemaphoreSlim _pendingOrdersReloadLock = new(1, 1);
    private readonly SemaphoreSlim _serviceRequestsReloadLock = new(1, 1);
    private readonly SemaphoreSlim _apiReloadLock = new(1, 1);
    private readonly List<TableCard> _tables;
    private readonly List<MenuItemCard> _menuItems;
    private readonly HashSet<string> _selectedAreas = [];
    private readonly HashSet<string> _selectedStatuses = [];
    private readonly List<PendingOrderCard> _allPendingOrders = [];
    private readonly List<ServiceRequestCard> _allServiceRequests = [];
    private string? _selectedCategory;
    private bool _usesApiData;
    private bool _isInitialized;
    private bool _isDisposed;
    private bool _isAddingMenuItem;
    private string _loadedMenuSalesChannelCode = "DINE_IN";
    private TableCard? _selectedTable;
    private BillPreview? _selectedBill;
    private MenuItemCard? _pendingChoiceItem;
    private ChoiceGroupCard? _activeChoiceGroup;

    public ObservableCollection<TableCard> FilteredTables { get; } = [];
    public ObservableCollection<MenuItemCard> FilteredMenuItems { get; } = [];
    public ObservableCollection<FilterOption> AreaFilterOptions { get; } = [];
    public ObservableCollection<FilterOption> CategoryFilterOptions { get; } = [];
    public ObservableCollection<BillPreview> CurrentBills { get; } = [];
    public ObservableCollection<BillLinePreview> CurrentBillLines { get; } = [];
    public ObservableCollection<PendingOrderCard> PendingOrders { get; } = [];
    public ObservableCollection<ServiceRequestCard> ServiceRequests { get; } = [];
    public ObservableCollection<ChoiceGroupCard> PendingChoiceGroups { get; } = [];
    public ObservableCollection<ChoiceOptionCard> ActiveChoiceOptions { get; } = [];

    public string MenuPageText => $"1/{Math.Max(1, (int)Math.Ceiling(FilteredMenuItems.Count / 12d))}";

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<int>? SidebarNotificationCountChanged;

    internal OrderManagementView(
        StaffOrderApiClient apiClient,
        StaffRealtimeClient realtimeClient,
        PdfDemoPrintService pdfPrintService,
        IMenuManagementService menuManagementService,
        IBillService billService)
    {
        _apiClient = apiClient;
        _realtimeClient = realtimeClient;
        _pdfPrintService = pdfPrintService;
        _menuManagementService = menuManagementService;
        _billService = billService;
        InitializeComponent();
        _tables = [];
        _menuItems = [];
        DataContext = this;

        ApplyTableFilters();
        ApplyMenuFilters();
        RefreshBill();
        RegisterRealtimeHandlers();
        Loaded += OrderManagementView_Loaded;
    }

    private async void OrderManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized || _isDisposed)
        {
            return;
        }

        _isInitialized = true;
        try
        {
            await _realtimeClient.StartAsync();
        }
        catch
        {
            // Realtime is a convenience layer; the manual refresh buttons remain available.
        }

        await LoadFromApiAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        await _realtimeClient.DisposeAsync();
        _apiClient.Dispose();
    }

    private async Task LoadFromApiAsync()
    {
        if (!await _apiReloadLock.WaitAsync(0))
        {
            return;
        }

        try
        {
            IReadOnlyList<DiningTableDto> tables = await _apiClient.GetTablesAsync();
            MenuCatalogDto catalog = await _apiClient.GetMenuCatalogAsync();

            Dictionary<int, string> categoryNames = catalog.Categories
                .ToDictionary(x => x.CategoryId, x => x.CategoryName);
            Dictionary<int, int> categoryOrders = catalog.Categories
                .ToDictionary(x => x.CategoryId, x => x.DisplayOrder);

            List<TableCard> loadedTables = [];

            foreach (DiningTableDto tableDto in tables
                         .OrderBy(x => x.AreaDisplayOrder)
                         .ThenBy(x => x.Area)
                         .ThenBy(x => x.TableDisplayOrder)
                         .ThenBy(x => x.TableName))
            {
                TableCard table = new(
                    tableDto.TableId,
                    tableDto.CurrentTableSessionId,
                    tableDto.TableName,
                    tableDto.Area,
                    tableDto.Status,
                    tableDto.AreaDisplayOrder,
                    tableDto.TableDisplayOrder);

                if (tableDto.CurrentTableSessionId.HasValue)
                {
                    await LoadBillsForTableAsync(table, tableDto.CurrentTableSessionId.Value);
                }

                loadedTables.Add(table);
            }

            List<MenuItemCard> loadedMenuItems = catalog.Items
                .OrderBy(x => categoryOrders.TryGetValue(x.CategoryId, out int categoryOrder) ? categoryOrder : int.MaxValue)
                .ThenBy(x => categoryNames.TryGetValue(x.CategoryId, out string? categoryName) ? categoryName : string.Empty)
                .ThenBy(x => x.Name)
                .Select(x => MapMenuItem(x, categoryNames))
                .ToList();

            _usesApiData = true;
            _tables.Clear();
            _tables.AddRange(loadedTables);
            _menuItems.Clear();
            _menuItems.AddRange(loadedMenuItems);
            _loadedMenuSalesChannelCode = "DINE_IN";
            RebuildAreaFilters();
            RebuildCategoryFilters(catalog.Categories);

            _selectedTable = _selectedTable is null
                ? null
                : _tables.FirstOrDefault(x => x.TableId == _selectedTable.TableId);
            _selectedBill = null;

            if (_selectedTable is not null)
            {
                SelectTable(_selectedTable);
            }
            else
            {
                SelectedTableHeaderButton.Content = "Chưa chọn bàn";
                CurrentBills.Clear();
                RefreshBill();
            }

            ApplyTableFilters();
            ApplyMenuFilters();
            await LoadPendingOrdersAsync();
            await LoadServiceRequestsAsync();
        }
        catch
        {
            _usesApiData = false;
            _tables.Clear();
            _menuItems.Clear();
            AreaFilterOptions.Clear();
            CategoryFilterOptions.Clear();
            FilteredTables.Clear();
            FilteredMenuItems.Clear();
            PendingOrders.Clear();
            ServiceRequests.Clear();
            CurrentBills.Clear();
            CurrentBillLines.Clear();
            _selectedTable = null;
            _selectedBill = null;
            SelectedTableHeaderButton.Content = "Không tải được dữ liệu";
            RefreshBill();
            OnPropertyChanged(nameof(MenuPageText));
        }
        finally
        {
            _apiReloadLock.Release();
        }
    }

    private void RebuildAreaFilters()
    {
        HashSet<string> availableAreas = _tables
            .Select(x => x.Area)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _selectedAreas.RemoveWhere(area => !availableAreas.Contains(area));

        AreaFilterOptions.Clear();
        foreach (FilterOption option in _tables
                     .Where(x => !string.IsNullOrWhiteSpace(x.Area))
                     .GroupBy(x => x.Area)
                     .OrderBy(group => group.Min(x => x.AreaDisplayOrder))
                     .ThenBy(group => group.Key)
                     .Select(group => new FilterOption(group.Key, DisplayArea(group.Key))))
        {
            option.IsActive = _selectedAreas.Contains(option.Value);
            AreaFilterOptions.Add(option);
        }

        AllAreaButton.Tag = _selectedAreas.Count == 0 ? "Active" : null;
    }

    private void RebuildCategoryFilters(IEnumerable<MenuCategoryDto> categories)
    {
        HashSet<string> availableCategories = categories
            .Select(x => x.CategoryName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (_selectedCategory is not null && !availableCategories.Contains(_selectedCategory))
        {
            _selectedCategory = null;
        }

        CategoryFilterOptions.Clear();
        foreach (MenuCategoryDto category in categories.OrderBy(x => x.DisplayOrder).ThenBy(x => x.CategoryName))
        {
            FilterOption option = new(category.CategoryName, category.CategoryName)
            {
                IsActive = string.Equals(_selectedCategory, category.CategoryName, StringComparison.OrdinalIgnoreCase)
            };
            CategoryFilterOptions.Add(option);
        }

        AllCategoryButton.Tag = string.IsNullOrWhiteSpace(_selectedCategory) ? "Active" : null;
    }




    private static string ColorFromId(int id)
    {
        string[] colors =
        [
            "#F59E0B",
            "#F97316",
            "#7C2D12",
            "#EAB308",
            "#DC2626",
            "#22C55E",
            "#0EA5E9",
            "#A855F7"
        ];

        return colors[Math.Abs(id) % colors.Length];
    }

    private static System.Windows.Media.Brush GetBrush(string hex) => (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(hex)!;

    private static string GetFriendlyError(Exception exception)
    {
        string message = exception.Message;
        return string.IsNullOrWhiteSpace(message) ? "Thao tác không thành công." : message;
    }
















        private async void RequestsTabButton_Click(object sender, RoutedEventArgs e)
    {
        RoomPanel.Visibility = Visibility.Collapsed;
        MenuPanel.Visibility = Visibility.Collapsed;
        PendingOrdersPanel.Visibility = Visibility.Collapsed;
        RequestsPanel.Visibility = Visibility.Visible;
        RoomTabButton.Tag = null;
        MenuTabButton.Tag = null;
        PendingOrdersTabButton.Tag = null;
        RequestsTabButton.Tag = "Active";
        SearchBox.Text = string.Empty;
        SetSearchContext("Tìm kiếm yêu cầu...");
        await LoadServiceRequestsAsync();
    }












































































            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }











}
