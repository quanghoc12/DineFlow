using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Tables;

namespace DineFlow.DataAccessObjects.Orders;

public interface ITableSessionDao
{
    Task<IReadOnlyList<DiningTable>> GetDiningTablesAsync(
        bool activeOnly,
        string? status,
        string? area,
        CancellationToken cancellationToken = default);
    Task<DiningTable?> GetActiveTableByIdAsync(int tableId, CancellationToken cancellationToken = default);
    Task<DiningTable?> GetActiveTableByQrTokenAsync(string qrToken, CancellationToken cancellationToken = default);
    Task<DiningTable?> GetTableByIdAsync(int tableId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TableSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    Task<TableSessionCustomer?> GetSessionCustomerAsync(
        int tableSessionId,
        string clientToken,
        CancellationToken cancellationToken = default);
    Task<TableSessionCustomer?> GetSessionCustomerByTokenAsync(
        string clientToken,
        CancellationToken cancellationToken = default);
    Task<TableSession?> GetByIdAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<TableSession?> GetCurrentByTableIdAsync(int tableId, CancellationToken cancellationToken = default);
    Task<TableSession?> GetCurrentCustomerSessionByTableIdAsync(int tableId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TableSession>> GetExpiredBrowsingSessionsAsync(
        DateTime expiresBefore,
        CancellationToken cancellationToken = default);
    Task AddTableSessionAsync(TableSession tableSession, CancellationToken cancellationToken = default);
    Task AddSessionCustomerAsync(TableSessionCustomer sessionCustomer, CancellationToken cancellationToken = default);
}
