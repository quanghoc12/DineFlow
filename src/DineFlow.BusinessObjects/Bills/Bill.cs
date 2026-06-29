using DineFlow.BusinessObjects.Orders;

namespace DineFlow.BusinessObjects.Bills;

public class Bill
{
    public int BillId { get; set; }
    public string BillCode { get; set; } = string.Empty;
    public int TableSessionId { get; set; }
    public int BillNo { get; set; }
    public string BillName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "Unpaid";
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledBy { get; set; }
    public string? CancelReason { get; set; }

    public TableSession? TableSession { get; set; }
    public ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
