namespace DineFlow.BusinessObjects.Menu;

public class MenuItemChoiceGroup
{
    public int MenuItemId { get; set; }
    public int ChoiceGroupId { get; set; }
    public int DisplayOrder { get; set; }
    public int? MaxSelect { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public MenuItem? MenuItem { get; set; }
    public ChoiceGroup? ChoiceGroup { get; set; }
}
