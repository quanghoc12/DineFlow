using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class Category : BaseEntity
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
