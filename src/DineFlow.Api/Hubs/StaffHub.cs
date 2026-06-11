using Microsoft.AspNetCore.SignalR;

namespace DineFlow.Api.Hubs;

public class StaffHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "staff");
        await base.OnConnectedAsync();
    }
}
