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
            ToggleArea(button.CommandParameter?.ToString() ?? "All");
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
            .OrderBy(x => x.AreaDisplayOrder)
            .ThenBy(x => x.Area)
            .ThenBy(x => x.TableDisplayOrder)
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
        if (!_usesApiData)
        {
            throw new InvalidOperationException("Chưa kết nối được dữ liệu database. Vui lòng chạy API và tải lại màn Order.");
        }

        if (_selectedTable is not null)
        {
            return _selectedTable;
        }

        if (_tables.Count == 0)
        {
            throw new InvalidOperationException("Chưa có bàn nào trong database. Vui lòng thêm bàn trong Quản lý bàn trước.");
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

    private void ToggleArea(string area)
    {
        if (area == "All")
        {
            _selectedAreas.Clear();
            AllAreaButton.Tag = "Active";
            foreach (FilterOption option in AreaFilterOptions)
            {
                option.IsActive = false;
            }
            return;
        }

        AllAreaButton.Tag = null;

        if (!_selectedAreas.Add(area))
        {
            _selectedAreas.Remove(area);
        }

        foreach (FilterOption option in AreaFilterOptions)
        {
            option.IsActive = _selectedAreas.Contains(option.Value);
        }

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

    private static string DisplayArea(string area) => area;

}
