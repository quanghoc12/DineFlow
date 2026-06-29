using DineFlow.Api.Hubs;
using DineFlow.Services.CustomerSessions;
using DineFlow.Services.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace DineFlow.Api.Realtime;

public class SignalRRealtimeNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<DineFlowHub> _hubContext;
    private readonly ILogger<SignalRRealtimeNotificationService> _logger;

    public SignalRRealtimeNotificationService(
        IHubContext<DineFlowHub> hubContext,
        ILogger<SignalRRealtimeNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyStaffAsync(
        string eventName,
        RealtimeEventDto payload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(DineFlowHub.StaffGroup)
                .SendAsync(eventName, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send SignalR staff event {EventName}.", eventName);
        }
    }

    public async Task NotifyCustomerAsync(
        string clientToken,
        string eventName,
        CustomerMessageDto payload,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientToken))
        {
            return;
        }

        try
        {
            await _hubContext.Clients
                .Group(DineFlowHub.CustomerGroup(clientToken))
                .SendAsync(eventName, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send SignalR customer event {EventName}.", eventName);
        }
    }

    public async Task NotifySessionAsync(
        int tableSessionId,
        string eventName,
        RealtimeEventDto payload,
        CancellationToken cancellationToken = default)
    {
        if (tableSessionId <= 0)
        {
            return;
        }

        try
        {
            await _hubContext.Clients
                .Group(DineFlowHub.SessionGroup(tableSessionId))
                .SendAsync(eventName, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send SignalR session event {EventName}.", eventName);
        }
    }
}
