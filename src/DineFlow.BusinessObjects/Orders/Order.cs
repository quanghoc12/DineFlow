using DineFlow.BusinessObjects.Menu;

namespace DineFlow.BusinessObjects.Orders;

public class Order
{
    public int OrderId { get; set; }
    public int TableSessionId { get; set; }
    public int? SessionCustomerId { get; set; }
    public int SalesChannelId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string OrderSource { get; set; } = string.Empty;
    public string? ExternalOrderCode { get; set; }
    public string? ClientToken { get; set; }
    public string Status { get; set; } = "Accepted";
    public string? PrintStatus { get; set; }
    public string? CustomerNote { get; set; }
    public string? SystemNote { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? PrintedAt { get; set; }
    public string? PrintError { get; set; }
    public int? CreatedBy { get; set; }
    public int? CancelledBy { get; set; }

    public TableSession? TableSession { get; set; }
    public TableSessionCustomer? SessionCustomer { get; set; }
    public SalesChannel? SalesChannel { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
