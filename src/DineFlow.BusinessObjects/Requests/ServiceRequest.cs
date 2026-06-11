using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Tables;

namespace DineFlow.BusinessObjects.Requests;

public class ServiceRequest
{
    public int RequestId { get; set; }
    public int TableSessionId { get; set; }
    public int? SessionCustomerId { get; set; }
    public string? ClientToken { get; set; }
    public ServiceRequestType RequestType { get; set; }
    public string? Reason { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? Message { get; set; }
    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ConfirmedBy { get; set; }
    public int? CompletedBy { get; set; }

    public TableSession? TableSession { get; set; }
    public TableSessionCustomer? SessionCustomer { get; set; }
    public User? ConfirmedByUser { get; set; }
    public User? CompletedByUser { get; set; }
}
