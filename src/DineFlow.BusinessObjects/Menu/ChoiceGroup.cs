using DineFlow.BusinessObjects.Orders;

namespace DineFlow.BusinessObjects.Menu;

public class ChoiceGroup
{
    public int ChoiceGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public bool IsRequired { get; set; }
    public int MaxSelectDefault { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ChoiceItem> ChoiceItems { get; set; } = new List<ChoiceItem>();
    public ICollection<MenuItemChoiceGroup> MenuItemChoiceGroups { get; set; } = new List<MenuItemChoiceGroup>();
    public ICollection<OrderItemSelectedChoice> OrderItemSelectedChoices { get; set; } = new List<OrderItemSelectedChoice>();
}
