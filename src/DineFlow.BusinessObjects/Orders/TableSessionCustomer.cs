using DineFlow.BusinessObjects.Requests;

namespace DineFlow.BusinessObjects.Orders;

public class TableSessionCustomer
{
    public int SessionCustomerId { get; set; }
    public int TableSessionId { get; set; }
    public string ClientToken { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public TableSession? TableSession { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
