using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Bills;

public class Payment
{
    public int PaymentId { get; set; }
    public int BillId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public int ConfirmedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public string? ChangeReason { get; set; }

    public Bill? Bill { get; set; }
    public User? ConfirmedByUser { get; set; }
    public User? UpdatedByUser { get; set; }
}
