namespace DineFlow.Services.Orders;

public class TableSessionDto
{
    public int TableSessionId { get; set; }
    public int TableId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TableSessionDetailDto : TableSessionDto
{
    public List<OrderSummaryDto> Orders { get; set; } = [];
}

public class DiningTableDto
{
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int? AreaId { get; set; }
    public string Area { get; set; } = string.Empty;
    public int AreaDisplayOrder { get; set; }
    public int TableDisplayOrder { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? CurrentTableSessionId { get; set; }
    public string? CurrentSessionStatus { get; set; }
}

public class DiningTableFilter
{
    public string? Status { get; set; }
    public string? Area { get; set; }
    public bool ActiveOnly { get; set; } = true;
}

public class MoveTableSessionRequest
{
    public int TargetTableId { get; set; }
}

public interface ITableSessionService
{
    Task<IReadOnlyList<DiningTableDto>> GetTablesAsync(DiningTableFilter filter, CancellationToken cancellationToken = default);
    Task<TableSessionDto?> GetByIdAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<TableSessionDto?> GetCurrentSessionByTableIdAsync(int tableId, CancellationToken cancellationToken = default);
    Task<TableSessionDto> GetOrCreateActiveSessionByTableIdAsync(int tableId, int? openedBy, CancellationToken cancellationToken = default);
    Task<TableSessionDto> GetOrCreateActiveSessionByQrTokenAsync(string qrToken, int? openedBy, CancellationToken cancellationToken = default);
    Task<TableSessionDto> ActivateBrowsingSessionAsync(int tableSessionId, int openedBy, CancellationToken cancellationToken = default);
    Task<int> ExpireInactiveBrowsingSessionsAsync(CancellationToken cancellationToken = default);
    Task<TableSessionDto> MarkWaitingPaymentAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<bool> CloseSessionIfCompletedAsync(int tableSessionId, int closedBy, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TableSessionDto>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    Task<TableSessionDetailDto?> GetSessionDetailAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<TableSessionDto> MoveTableAsync(int tableSessionId, MoveTableSessionRequest request, CancellationToken cancellationToken = default);
}
