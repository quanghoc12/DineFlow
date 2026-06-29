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

public partial class OrderManagementView : UserControl, INotifyPropertyChanged
{
    private readonly StaffOrderApiClient _apiClient;
    private readonly StaffRealtimeClient _realtimeClient;
    private readonly PdfDemoPrintService _pdfPrintService;
    private readonly SemaphoreSlim _billReloadLock = new(1, 1);
    private readonly SemaphoreSlim _pendingOrdersReloadLock = new(1, 1);
    private readonly SemaphoreSlim _serviceRequestsReloadLock = new(1, 1);
    private readonly List<TableCard> _tables;
    private readonly List<MenuItemCard> _menuItems;
    private readonly HashSet<string> _selectedAreas = [];
    private readonly HashSet<string> _selectedStatuses = [];
    private readonly List<PendingOrderCard> _allPendingOrders = [];
    private readonly List<ServiceRequestCard> _allServiceRequests = [];
    private string? _selectedCategory;
    private bool _usesApiData;
    private bool _isAddingMenuItem;
    private TableCard? _selectedTable;
    private BillPreview? _selectedBill;
    private MenuItemCard? _pendingChoiceItem;
    private ChoiceGroupCard? _activeChoiceGroup;

    public ObservableCollection<TableCard> FilteredTables { get; } = [];
    public ObservableCollection<MenuItemCard> FilteredMenuItems { get; } = [];
    public ObservableCollection<BillPreview> CurrentBills { get; } = [];
    public ObservableCollection<BillLinePreview> CurrentBillLines { get; } = [];
    public ObservableCollection<PendingOrderCard> PendingOrders { get; } = [];
    public ObservableCollection<ServiceRequestCard> ServiceRequests { get; } = [];
    public ObservableCollection<ChoiceGroupCard> PendingChoiceGroups { get; } = [];
    public ObservableCollection<ChoiceOptionCard> ActiveChoiceOptions { get; } = [];

    public string MenuPageText => $"1/{Math.Max(1, (int)Math.Ceiling(FilteredMenuItems.Count / 12d))}";

    public event PropertyChangedEventHandler? PropertyChanged;

    internal OrderManagementView(
        StaffOrderApiClient apiClient,
        StaffRealtimeClient realtimeClient,
        PdfDemoPrintService pdfPrintService)
    {
        _apiClient = apiClient;
        _realtimeClient = realtimeClient;
        _pdfPrintService = pdfPrintService;
        InitializeComponent();
        _tables = CreateMockTables();
        _menuItems = CreateMockMenuItems();
        DataContext = this;

        ApplyTableFilters();
        ApplyMenuFilters();
        RefreshBill();
        _ = LoadFromApiAsync();
        RegisterRealtimeHandlers();
        Loaded += OrderManagementView_Loaded;
        Unloaded += OrderManagementView_Unloaded;
    }

    private async void OrderManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _realtimeClient.StartAsync();
        }
        catch
        {
            // Realtime is a convenience layer; the manual refresh buttons remain available.
        }
    }

    private async void OrderManagementView_Unloaded(object sender, RoutedEventArgs e)
    {
        await _realtimeClient.DisposeAsync();
    }

    private async Task LoadFromApiAsync()
    {
        try
        {
            IReadOnlyList<DiningTableDto> tables = await _apiClient.GetTablesAsync();
            MenuCatalogDto catalog = await _apiClient.GetMenuCatalogAsync();

            Dictionary<int, string> categoryNames = catalog.Categories
                .ToDictionary(x => x.CategoryId, x => x.CategoryName);

            List<TableCard> loadedTables = [];

            foreach (DiningTableDto tableDto in tables.OrderBy(x => x.Area).ThenBy(x => x.TableName))
            {
                TableCard table = new(
                    tableDto.TableId,
                    tableDto.CurrentTableSessionId,
                    tableDto.TableName,
                    tableDto.Area,
                    tableDto.Status);

                if (tableDto.CurrentTableSessionId.HasValue)
                {
                    await LoadBillsForTableAsync(table, tableDto.CurrentTableSessionId.Value);
                }

                loadedTables.Add(table);
            }

            List<MenuItemCard> loadedMenuItems = catalog.Items
                .OrderBy(x => categoryNames.TryGetValue(x.CategoryId, out string? categoryName) ? categoryName : string.Empty)
                .ThenBy(x => x.Name)
                .Select(x => MapMenuItem(x, categoryNames))
                .ToList();

            if (loadedTables.Count == 0 || loadedMenuItems.Count == 0)
            {
                return;
            }

            _usesApiData = true;
            _tables.Clear();
            _tables.AddRange(loadedTables);
            _menuItems.Clear();
            _menuItems.AddRange(loadedMenuItems);

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
        }
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
