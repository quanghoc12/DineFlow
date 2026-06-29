using DineFlow.BusinessObjects.Menu;

namespace DineFlow.BusinessObjects.Bills;

public class BillDetail
{
    public int BillDetailId { get; set; }
    public int BillId { get; set; }
    public int MenuItemId { get; set; }
    public int SalesChannelId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ChoiceSummary { get; set; }
    public string? Note { get; set; }
    public int Quantity { get; set; }
    public int NotifiedQuantity { get; set; }
    public decimal BasePriceSnapshot { get; set; }
    public decimal MenuItemChannelExtraPriceSnapshot { get; set; }
    public decimal ChoiceExtraPriceSnapshot { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public Bill? Bill { get; set; }
    public MenuItem? MenuItem { get; set; }
}
