using DineFlow.BusinessObjects.Orders;

namespace DineFlow.BusinessObjects.Requests;

public class ServiceRequest
{
    public int RequestId { get; set; }
    public int TableSessionId { get; set; }
    public int? SessionCustomerId { get; set; }
    public string? ClientToken { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "Confirmed";
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public int? ConfirmedBy { get; set; }

    public TableSession? TableSession { get; set; }
    public TableSessionCustomer? SessionCustomer { get; set; }
}
