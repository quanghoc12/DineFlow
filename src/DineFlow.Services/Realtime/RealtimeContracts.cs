using DineFlow.Services.CustomerSessions;

namespace DineFlow.Services.Realtime;

public static class RealtimeEvents
{
    public const string CustomerMessageCreated = "CustomerMessageCreated";
    public const string CustomerOrderCreated = "CustomerOrderCreated";
    public const string CustomerOrderStatusChanged = "CustomerOrderStatusChanged";
    public const string ServiceRequestCreated = "ServiceRequestCreated";
    public const string ServiceRequestConfirmed = "ServiceRequestConfirmed";
    public const string TableSessionChanged = "TableSessionChanged";
    public const string TableOtpChanged = "TableOtpChanged";
    public const string BillChanged = "BillChanged";
    public const string PaymentConfirmed = "PaymentConfirmed";
}

public class RealtimeEventDto
{
    public int TableSessionId { get; set; }
    public int? TableId { get; set; }
    public int? OrderId { get; set; }
    public int? RequestId { get; set; }
    public int? BillId { get; set; }
    public string? CurrentOtp { get; set; }
    public DateTime? OtpUpdatedAt { get; set; }
    public string? TableStatus { get; set; }
    public string? SessionStatus { get; set; }
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
}

public interface IRealtimeNotificationService
{
    Task NotifyStaffAsync(string eventName, RealtimeEventDto payload, CancellationToken cancellationToken = default);
    Task NotifyCustomerAsync(string clientToken, string eventName, CustomerMessageDto payload, CancellationToken cancellationToken = default);
    Task NotifySessionAsync(int tableSessionId, string eventName, RealtimeEventDto payload, CancellationToken cancellationToken = default);
}

public class NullRealtimeNotificationService : IRealtimeNotificationService
{
    public Task NotifyStaffAsync(string eventName, RealtimeEventDto payload, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifyCustomerAsync(
        string clientToken,
        string eventName,
        CustomerMessageDto payload,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task NotifySessionAsync(
        int tableSessionId,
        string eventName,
        RealtimeEventDto payload,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
