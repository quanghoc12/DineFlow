using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.Orders;

namespace DineFlow.Repositories.Orders;

public class TableSessionRepository : ITableSessionRepository
{
    private readonly ITableSessionDao _tableSessionDao;

    public TableSessionRepository(ITableSessionDao tableSessionDao)
    {
        _tableSessionDao = tableSessionDao;
    }

    public async Task<IReadOnlyList<DiningTable>> GetDiningTablesAsync(
        bool activeOnly,
        string? status,
        string? area,
        CancellationToken cancellationToken = default)
    {
        return await _tableSessionDao.GetDiningTablesAsync(activeOnly, status, area, cancellationToken);
    }

    public Task<DiningTable?> GetActiveTableByIdAsync(int tableId, CancellationToken cancellationToken = default)
    {
        return _tableSessionDao.GetActiveTableByIdAsync(tableId, cancellationToken);
    }

    public Task<DiningTable?> GetActiveTableByQrTokenAsync(string qrToken, CancellationToken cancellationToken = default)
    {
        return _tableSessionDao.GetActiveTableByQrTokenAsync(qrToken, cancellationToken);
    }

    public Task<DiningTable?> GetTableByIdAsync(int tableId, CancellationToken cancellationToken = default)
    {
        return _tableSessionDao.GetTableByIdAsync(tableId, cancellationToken);
    }

    public async Task<IReadOnlyList<TableSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _tableSessionDao.GetActiveSessionsAsync(cancellationToken);
    }

    public Task<TableSessionCustomer?> GetSessionCustomerAsync(
        int tableSessionId,
        string clientToken,
        CancellationToken cancellationToken = default)
    {
        return _tableSessionDao.GetSessionCustomerAsync(tableSessionId, clientToken, cancellationToken);
    }

    public Task<TableSessionCustomer?> GetSessionCustomerByTokenAsync(
        string clientToken,
        CancellationToken cancellationToken = default)
    {
        return _tableSessionDao.GetSessionCustomerByTokenAsync(clientToken, cancellationToken);
    }

    public Task<TableSession?> GetByIdAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return _tableSessionDao.GetByIdAsync(tableSessionId, cancellationToken);
    }

    public Task<TableSession?> GetCurrentByTableIdAsync(int tableId, CancellationToken cancellationToken = default)
    {
        return _tableSessionDao.GetCurrentByTableIdAsync(tableId, cancellationToken);
    }

    public Task<TableSession?> GetCurrentCustomerSessionByTableIdAsync(
        int tableId,
        CancellationToken cancellationToken = default)
    {
        return _tableSessionDao.GetCurrentCustomerSessionByTableIdAsync(tableId, cancellationToken);
    }

    public Task<IReadOnlyList<TableSession>> GetExpiredBrowsingSessionsAsync(
        DateTime expiresBefore,
        CancellationToken cancellationToken = default)
    {
        return _tableSessionDao.GetExpiredBrowsingSessionsAsync(expiresBefore, cancellationToken);
    }

    public async Task AddTableSessionAsync(TableSession tableSession, CancellationToken cancellationToken = default)
    {
        await _tableSessionDao.AddTableSessionAsync(tableSession, cancellationToken);
    }

    public async Task AddSessionCustomerAsync(TableSessionCustomer sessionCustomer, CancellationToken cancellationToken = default)
    {
        await _tableSessionDao.AddSessionCustomerAsync(sessionCustomer, cancellationToken);
    }
}
