namespace DineFlow.BusinessObjects.Bills;

public class Payment
{
    public int PaymentId { get; set; }
    public int BillId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public int ConfirmedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public string? ChangeReason { get; set; }

    public Bill? Bill { get; set; }
}
