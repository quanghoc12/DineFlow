using DineFlow.BusinessObjects.Orders;

namespace DineFlow.BusinessObjects.Tables;

public class DiningTable
{
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int? AreaId { get; set; }
    public string Area { get; set; } = string.Empty;
    public string QrToken { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Area? AreaEntity { get; set; }
    public ICollection<TableSession> TableSessions { get; set; } = new List<TableSession>();
}
