using DineFlow.BusinessObjects.Bills;
using DineFlow.BusinessObjects.Requests;
using DineFlow.BusinessObjects.Tables;

namespace DineFlow.BusinessObjects.Orders;

public class TableSession
{
    public int TableSessionId { get; set; }
    public int TableId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = "Open";
    public int? OpenedBy { get; set; }
    public int? ClosedBy { get; set; }

    public DiningTable? Table { get; set; }
    public ICollection<TableSessionCustomer> Customers { get; set; } = new List<TableSessionCustomer>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
