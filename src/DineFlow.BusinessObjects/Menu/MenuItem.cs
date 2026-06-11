using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Menu;

public class MenuItem : BaseEntity
{
    public int MenuItemId { get; set; }
    public int CategoryId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public bool TrackStock { get; set; }
    public int? AvailableQuantity { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Category? Category { get; set; }
}
