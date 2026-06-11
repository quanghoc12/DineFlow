using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Tables;

namespace DineFlow.BusinessObjects.Bills;

public class BillDetail
{
    public int BillDetailId { get; set; }
    public int BillId { get; set; }
    public int OrderItemId { get; set; }
    public int MenuItemId { get; set; }
    public int? SessionCustomerId { get; set; }
    public string? CustomerDisplayName { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public Bill? Bill { get; set; }
    public OrderItem? OrderItem { get; set; }
    public MenuItem? MenuItem { get; set; }
    public TableSessionCustomer? SessionCustomer { get; set; }
}
