using Microsoft.AspNetCore.SignalR;

namespace DineFlow.Api.Hubs;

public class CustomerHub : Hub
{
    public async Task JoinCustomerGroup(string clientToken)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{clientToken}");
    }

    public async Task JoinTableGroup(int tableId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"table-{tableId}");
    }
}
