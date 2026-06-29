using DineFlow.BusinessObjects.Bills;
using DineFlow.BusinessObjects.Orders;

namespace DineFlow.BusinessObjects.Menu;

public class MenuItem
{
    public int MenuItemId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int? Stock { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Category? Category { get; set; }
    public ICollection<MenuItemChoiceGroup> MenuItemChoiceGroups { get; set; } = new List<MenuItemChoiceGroup>();
    public ICollection<MenuItemChannelPrice> ChannelPrices { get; set; } = new List<MenuItemChannelPrice>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();
}
