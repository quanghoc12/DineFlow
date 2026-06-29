namespace DineFlow.BusinessObjects.Bills;

public class BillDetailAdjustment
{
    public int BillDetailAdjustmentId { get; set; }
    public int BillId { get; set; }
    public int BillDetailId { get; set; }
    public int MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public int ChangedQuantity { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }

    public Bill? Bill { get; set; }
}
