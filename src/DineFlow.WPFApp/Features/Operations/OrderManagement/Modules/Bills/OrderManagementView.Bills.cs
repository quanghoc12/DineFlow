using DineFlow.Services.Bills;
using DineFlow.Services.Orders;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{
    private async Task LoadBillsForTableAsync(TableCard table, int tableSessionId)
    {
        await _billReloadLock.WaitAsync();
        try
        {
            IReadOnlyList<BillSummaryDto> summaries = await _apiClient.GetBillsBySessionAsync(tableSessionId);

            List<BillPreview> loadedBills = [];
            foreach (BillSummaryDto summary in summaries
                .Where(x => x.Status == "Unpaid")
                .GroupBy(x => x.BillId)
                .Select(x => x.First())
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.BillNo)
                .ThenBy(x => x.BillId))
            {
                BillDto? billDto = await _apiClient.GetBillAsync(summary.BillId);
                if (billDto is not null)
                {
                    loadedBills.Add(MapBill(billDto));
                }
            }

            IReadOnlyList<BillPreview> normalizedBills = NormalizeLoadedBills(loadedBills);
            table.Bills.Clear();
            foreach (BillPreview bill in normalizedBills)
            {
                table.Bills.Add(bill);
            }
        }
        finally
        {
            _billReloadLock.Release();
        }
    }

    private static IReadOnlyList<BillPreview> NormalizeLoadedBills(IReadOnlyList<BillPreview> bills)
    {
        List<BillPreview> normalized = bills
            .GroupBy(x => x.BillId)
            .Select(group => group.First())
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.BillNo)
            .ThenBy(x => x.BillId)
            .ToList();

        BillPreview? defaultBill = normalized.FirstOrDefault(x => x.IsDefault) ?? normalized.FirstOrDefault();
        foreach (BillPreview bill in normalized)
        {
            bill.IsDefault = bill == defaultBill;
            bill.NotifyChanged();
        }

        return normalized;
    }

    private async Task ReloadTableFromApiAsync(int tableId, int? selectedBillId = null)
    {
        IReadOnlyList<DiningTableDto> tableDtos = await _apiClient.GetTablesAsync();
        DiningTableDto? tableDto = tableDtos.FirstOrDefault(x => x.TableId == tableId);

        if (tableDto is null)
        {
            await LoadFromApiAsync();
            return;
        }

        TableCard? table = _tables.FirstOrDefault(x => x.TableId == tableId);

        if (table is null)
        {
            await LoadFromApiAsync();
            table = _tables.FirstOrDefault(x => x.TableId == tableId);
            if (table is null)
            {
                return;
            }
        }

        table.TableSessionId = tableDto.CurrentTableSessionId;
        table.Status = tableDto.Status;
        if (tableDto.CurrentTableSessionId.HasValue)
        {
            await LoadBillsForTableAsync(table, tableDto.CurrentTableSessionId.Value);
        }
        else
        {
            table.Bills.Clear();
        }

        table.NotifyChanged();
        ApplyTableFilters();

        BillPreview? selectedBill = selectedBillId.HasValue
            ? table.Bills.FirstOrDefault(x => x.BillId == selectedBillId.Value)
            : null;

        SelectTable(table, selectedBill);
    }

    private void UpdateSelectedBillFromDto(BillDto billDto)
    {
        if (_selectedTable is null)
        {
            return;
        }

        BillPreview mappedBill = MapBill(billDto);
        RemoveDuplicateBills(_selectedTable);
        BillPreview? existingBill = _selectedTable.Bills.FirstOrDefault(x => x.BillId == mappedBill.BillId);

        if (existingBill is null)
        {
            _selectedTable.Bills.Add(mappedBill);
            SetSelectedBill(mappedBill);
        }
        else
        {
            existingBill.CopyFrom(mappedBill);
            SetSelectedBill(existingBill);
        }

        ReloadCurrentBills(_selectedTable);
        RefreshBill();
    }

    private static void RemoveDuplicateBills(TableCard table)
    {
        List<BillPreview> uniqueBills = table.Bills
            .GroupBy(x => x.BillId)
            .Select(group => group.First())
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.BillNo)
            .ThenBy(x => x.BillId)
            .ToList();

        table.Bills.Clear();
        foreach (BillPreview bill in uniqueBills)
        {
            table.Bills.Add(bill);
        }
    }
}
