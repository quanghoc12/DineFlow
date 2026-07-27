using DineFlow.Services.Realtime;
using DineFlow.WPFApp.Services.Configuration;
using Microsoft.AspNetCore.SignalR.Client;

namespace DineFlow.WPFApp.Services.Realtime;

public sealed class StaffRealtimeClient : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public event Func<RealtimeEventDto, Task>? CustomerOrderCreated;
    public event Func<RealtimeEventDto, Task>? CustomerOrderStatusChanged;
    public event Func<RealtimeEventDto, Task>? ServiceRequestCreated;
    public event Func<RealtimeEventDto, Task>? ServiceRequestConfirmed;
    public event Func<RealtimeEventDto, Task>? TableSessionChanged;
    public event Func<RealtimeEventDto, Task>? BillChanged;
    public event Func<RealtimeEventDto, Task>? PaymentConfirmed;

    public StaffRealtimeClient(string? baseAddress = null)
    {
        baseAddress ??= AppClientSettings.ResolveApiBaseUrl();
        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseAddress.TrimEnd('/')}/hubs/dineflow")
            .WithAutomaticReconnect()
            .Build();

        RegisterHandlers();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync(cancellationToken);
        }

        await _connection.InvokeAsync("JoinStaff", cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    private void RegisterHandlers()
    {
        _connection.On<RealtimeEventDto>(
            RealtimeEvents.CustomerOrderCreated,
            payload => InvokeAsync(CustomerOrderCreated, payload));
        _connection.On<RealtimeEventDto>(
            RealtimeEvents.CustomerOrderStatusChanged,
            payload => InvokeAsync(CustomerOrderStatusChanged, payload));
        _connection.On<RealtimeEventDto>(
            RealtimeEvents.ServiceRequestCreated,
            payload => InvokeAsync(ServiceRequestCreated, payload));
        _connection.On<RealtimeEventDto>(
            RealtimeEvents.ServiceRequestConfirmed,
            payload => InvokeAsync(ServiceRequestConfirmed, payload));
        _connection.On<RealtimeEventDto>(
            RealtimeEvents.TableSessionChanged,
            payload => InvokeAsync(TableSessionChanged, payload));
        _connection.On<RealtimeEventDto>(
            RealtimeEvents.BillChanged,
            payload => InvokeAsync(BillChanged, payload));
        _connection.On<RealtimeEventDto>(
            RealtimeEvents.PaymentConfirmed,
            payload => InvokeAsync(PaymentConfirmed, payload));

        _connection.Reconnected += async _ =>
        {
            await _connection.InvokeAsync("JoinStaff");
        };
    }

    private static Task InvokeAsync(
        Func<RealtimeEventDto, Task>? handler,
        RealtimeEventDto payload)
    {
        return handler?.Invoke(payload) ?? Task.CompletedTask;
    }
}
