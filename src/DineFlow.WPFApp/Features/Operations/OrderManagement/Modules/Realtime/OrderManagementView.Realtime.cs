using System.Windows.Threading;
using DineFlow.Services.Realtime;

namespace DineFlow.WPFApp.Features.Operations.OrderManagement;

public partial class OrderManagementView
{
    private void RegisterRealtimeHandlers()
    {
        _realtimeClient.CustomerOrderCreated += _ => RunOnUiAsync(LoadPendingOrdersAsync);
        _realtimeClient.CustomerOrderStatusChanged += _ => RunOnUiAsync(LoadPendingOrdersAsync);
        _realtimeClient.ServiceRequestCreated += _ => RunOnUiAsync(LoadServiceRequestsAsync);
        _realtimeClient.ServiceRequestConfirmed += _ => RunOnUiAsync(LoadServiceRequestsAsync);
        _realtimeClient.TableSessionChanged += _ => RunOnUiAsync(LoadFromApiAsync);
        _realtimeClient.PaymentConfirmed += _ => RunOnUiAsync(LoadFromApiAsync);
        _realtimeClient.BillChanged += payload => RunOnUiAsync(() => HandleBillChangedAsync(payload));
    }

    private async Task RunOnUiAsync(Func<Task> action)
    {
        await Dispatcher.InvokeAsync(async () => await action(), DispatcherPriority.Background);
    }

    private async Task HandleBillChangedAsync(RealtimeEventDto payload)
    {
        if (_selectedTable?.TableSessionId == payload.TableSessionId)
        {
            await ReloadTableFromApiAsync(_selectedTable.TableId, _selectedBill?.BillId);
            return;
        }

        await LoadFromApiAsync();
    }
}
