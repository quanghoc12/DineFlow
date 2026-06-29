using Microsoft.AspNetCore.SignalR;

namespace DineFlow.Api.Hubs;

public class DineFlowHub : Hub
{
    public const string StaffGroup = "staff";

    public async Task JoinStaff()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, StaffGroup);
    }

    public async Task JoinCustomer(string clientToken)
    {
        if (string.IsNullOrWhiteSpace(clientToken))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroup(clientToken));
    }

    public async Task JoinSession(int tableSessionId)
    {
        if (tableSessionId <= 0)
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, SessionGroup(tableSessionId));
    }

    public async Task LeaveSession(int tableSessionId)
    {
        if (tableSessionId <= 0)
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SessionGroup(tableSessionId));
    }

    public static string CustomerGroup(string clientToken)
    {
        return $"customer:{clientToken.Trim()}";
    }

    public static string SessionGroup(int tableSessionId)
    {
        return $"session:{tableSessionId}";
    }
}
