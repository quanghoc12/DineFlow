using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Tables;

namespace DineFlow.BusinessObjects.Bills;

public class Bill : BaseEntity
{
    public int BillId { get; set; }
    public int TableSessionId { get; set; }
    public string BillCode { get; set; } = string.Empty;
    public int BillNo { get; set; }
    public string? BillName { get; set; }
    public bool IsDefault { get; set; }
    public BillStatus Status { get; set; } = BillStatus.Unpaid;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? CancelledBy { get; set; }

    public TableSession? TableSession { get; set; }
    public User? CreatedByUser { get; set; }
    public User? CancelledByUser { get; set; }
    public ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
    public Payment? Payment { get; set; }
}
