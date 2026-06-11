namespace DineFlow.BusinessObjects.Tables;

public class TableSessionCustomer
{
    public int SessionCustomerId { get; set; }
    public int TableSessionId { get; set; }
    public string ClientToken { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TableSession? TableSession { get; set; }
}
