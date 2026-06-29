using DineFlow.BusinessObjects.Orders;

namespace DineFlow.BusinessObjects.Menu;

public class ChoiceItem
{
    public int ChoiceItemId { get; set; }
    public int ChoiceGroupId { get; set; }
    public string ChoiceName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ChoiceGroup? ChoiceGroup { get; set; }
    public ICollection<ChoiceItemChannelPrice> ChannelPrices { get; set; } = new List<ChoiceItemChannelPrice>();
    public ICollection<OrderItemSelectedChoice> OrderItemSelectedChoices { get; set; } = new List<OrderItemSelectedChoice>();
}
