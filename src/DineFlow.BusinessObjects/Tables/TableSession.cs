using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Bills;
using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Requests;

namespace DineFlow.BusinessObjects.Tables;

public class TableSession
{
    public int TableSessionId { get; set; }
    public int TableId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public TableSessionStatus Status { get; set; } = TableSessionStatus.Open;
    public int? OpenedBy { get; set; }
    public int? ClosedBy { get; set; }

    public DiningTable? Table { get; set; }
    public User? OpenedByUser { get; set; }
    public User? ClosedByUser { get; set; }
    public ICollection<TableSessionCustomer> Customers { get; set; } = new List<TableSessionCustomer>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
}
