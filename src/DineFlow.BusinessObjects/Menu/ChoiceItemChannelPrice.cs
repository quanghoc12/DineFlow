namespace DineFlow.BusinessObjects.Menu;

public class ChoiceItemChannelPrice
{
    public int ChoiceItemId { get; set; }
    public int SalesChannelId { get; set; }
    public decimal ChannelExtraPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ChoiceItem? ChoiceItem { get; set; }
    public SalesChannel? SalesChannel { get; set; }
}
