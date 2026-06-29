using DineFlow.BusinessObjects.Menu;

namespace DineFlow.BusinessObjects.Orders;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemNameSnapshot { get; set; } = string.Empty;
    public decimal BasePriceSnapshot { get; set; }
    public decimal ChannelExtraPriceSnapshot { get; set; }
    public decimal FinalUnitPriceSnapshot { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Order? Order { get; set; }
    public MenuItem? MenuItem { get; set; }
    public ICollection<OrderItemSelectedChoice> SelectedChoices { get; set; } = new List<OrderItemSelectedChoice>();
}
