using DineFlow.BusinessObjects.Common;

namespace DineFlow.BusinessObjects.Tables;

public class DiningTable : BaseEntity
{
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string QrToken { get; set; } = string.Empty;
    public DiningTableStatus Status { get; set; } = DiningTableStatus.Available;
    public bool IsActive { get; set; } = true;

    public ICollection<TableSession> TableSessions { get; set; } = new List<TableSession>();
}
