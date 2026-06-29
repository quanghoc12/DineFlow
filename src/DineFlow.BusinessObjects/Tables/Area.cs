namespace DineFlow.BusinessObjects.Tables;

public class Area
{
    public int AreaId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<DiningTable> DiningTables { get; set; } = new List<DiningTable>();
}
