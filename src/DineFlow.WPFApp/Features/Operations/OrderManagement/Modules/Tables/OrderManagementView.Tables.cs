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
    private void AreaButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            ToggleArea(button.CommandParameter?.ToString() ?? "All", button);
            ApplyTableFilters();
        }
    }

    private void StatusButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            ToggleStatus(button.CommandParameter?.ToString() ?? "All", button);
            ApplyTableFilters();
        }
    }

    private void TableTile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: TableCard table })
        {
            SelectTable(table);
        }
    }

    private void SelectedTableHeaderButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTable is null || !_selectedTable.HasSession)
        {
            ShowCustomMessageBox("Chọn bàn đang có khách trước khi chuyển bàn.", "Chuyển bàn", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        List<TableCard> emptyTables = _tables
            .Where(x => x != _selectedTable && !x.HasSession)
            .OrderBy(x => x.Area)
            .ThenBy(x => x.TableName)
            .ToList();

        if (emptyTables.Count == 0)
        {
            ShowCustomMessageBox("Không còn bàn trống để chuyển.", "Chuyển bàn", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Window dialog = BuildMoveTableDialog(emptyTables);
        ShowBorderlessDialog(dialog);
    }

    private void SelectTable(TableCard table, BillPreview? selectedBill = null)
    {
        foreach (TableCard item in _tables)
        {
            item.IsSelected = item == table;
            item.NotifyChanged();
        }

        _selectedTable = table;
        SelectedTableHeaderButton.Content = $"{table.TableName} / {DisplayArea(table.Area)}";
        ReloadCurrentBills(table);
        SetSelectedBill(selectedBill ?? CurrentBills.FirstOrDefault(x => x.IsDefault) ?? CurrentBills.FirstOrDefault());
        RefreshBill();
    }

    private TableCard EnsureSelectedTable()
    {
        if (_selectedTable is not null)
        {
            return _selectedTable;
        }

        TableCard table = _tables.FirstOrDefault(x => x.HasSession) ?? _tables.First();
        SelectTable(table);
        return table;
    }

    private void ReloadCurrentBills(TableCard table)
    {
        CurrentBills.Clear();
        foreach (BillPreview bill in NormalizeLoadedBills(table.Bills.ToList()))
        {
            CurrentBills.Add(bill);
        }
    }

    private void ApplyTableFilters()
    {
        FilteredTables.Clear();

        IEnumerable<TableCard> tables = _tables;
        string keyword = SearchBox.Text.Trim();

        if (_selectedAreas.Count > 0)
        {
            tables = tables.Where(x => _selectedAreas.Contains(x.Area));
        }

        if (_selectedStatuses.Count > 0)
        {
            tables = tables.Where(x => _selectedStatuses.Contains(x.FilterStatus));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            tables = tables.Where(x =>
                x.TableName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.Area.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.DisplayStatus.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        foreach (TableCard table in tables)
        {
            FilteredTables.Add(table);
        }
    }

    private void ToggleArea(string area, Button button)
    {
        if (area == "All")
        {
            _selectedAreas.Clear();
            AllAreaButton.Tag = "Active";
            FloorOneButton.Tag = null;
            FloorTwoButton.Tag = null;
            VipAreaButton.Tag = null;
            return;
        }

        AllAreaButton.Tag = null;

        if (!_selectedAreas.Add(area))
        {
            _selectedAreas.Remove(area);
        }

        button.Tag = _selectedAreas.Contains(area) ? "Active" : null;

        if (_selectedAreas.Count == 0)
        {
            AllAreaButton.Tag = "Active";
        }
    }

    private void ToggleStatus(string status, Button button)
    {
        if (status == "All")
        {
            _selectedStatuses.Clear();
            AllStatusButton.Tag = "Active";
            EmptyStatusButton.Tag = null;
            ServingStatusButton.Tag = null;
            return;
        }

        AllStatusButton.Tag = null;

        if (!_selectedStatuses.Add(status))
        {
            _selectedStatuses.Remove(status);
        }

        button.Tag = _selectedStatuses.Contains(status) ? "Active" : null;

        if (_selectedStatuses.Count == 0)
        {
            AllStatusButton.Tag = "Active";
        }
    }

    private static IEnumerable<Button> FindSiblingButtons(Button button)
    {
        if (button.Parent is not Panel panel)
        {
            return [];
        }

        return panel.Children.OfType<Button>().Where(x => x != button);
    }

    private static List<TableCard> CreateMockTables()
    {
        TableCard tableOne = new("Bàn 01", "Tang 1", "Available");
        TableCard tableTwo = new("Bàn 02", "Tang 1", "Occupied");
        BillPreview tableTwoDefault = tableTwo.CreateNextBill(isDefault: true);
        tableTwoDefault.Lines.Add(new BillLinePreview(1, "APEROL SPRITZ", "Không có ghi chú/Món thêm", 1, 30000m));
        tableTwoDefault.Lines.Add(new BillLinePreview(2, "GIN FIZZ", "Không có ghi chú/Món thêm", 1, 30000m));

        BillPreview tableTwoSecond = tableTwo.CreateNextBill();
        tableTwoSecond.Lines.Add(new BillLinePreview(13, "Khoai tây chiên", "Không có ghi chú/Món thêm", 1, 35000m));

        TableCard tableThree = new("Bàn 03", "Tang 1", "WaitingPayment");
        BillPreview tableThreeDefault = tableThree.CreateNextBill(isDefault: true);
        tableThreeDefault.Lines.Add(new BillLinePreview(3, "Cơm gà xối mỡ", "Không có ghi chú/Món thêm", 2, 55000m));

        TableCard tableFour = new("Bàn 04", "Tang 2", "Available");
        TableCard tableFive = new("Bàn 05", "Tang 2", "Occupied");
        BillPreview tableFiveDefault = tableFive.CreateNextBill(isDefault: true);
        tableFiveDefault.Lines.Add(new BillLinePreview(4, "Bún thịt nướng", "Không có ghi chú/Món thêm", 1, 50000m));
        tableFiveDefault.Lines.Add(new BillLinePreview(5, "Nước suối", "Không có ghi chú/Món thêm", 2, 10000m));

        TableCard vip = new("VIP 01", "VIP", "Available");
        return [tableOne, tableTwo, tableThree, tableFour, tableFive, vip];
    }

    private static string DisplayArea(string area)
    {
        return area switch
        {
            "Tang 1" => "Tầng 1",
            "Tang 2" => "Tầng 2",
            _ => area
        };
    }

}
