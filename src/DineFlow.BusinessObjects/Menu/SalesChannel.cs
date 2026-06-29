using DineFlow.BusinessObjects.Orders;

namespace DineFlow.BusinessObjects.Menu;

public class SalesChannel
{
    public int SalesChannelId { get; set; }
    public string ChannelCode { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<MenuItemChannelPrice> MenuItemChannelPrices { get; set; } = new List<MenuItemChannelPrice>();
    public ICollection<ChoiceItemChannelPrice> ChoiceItemChannelPrices { get; set; } = new List<ChoiceItemChannelPrice>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
