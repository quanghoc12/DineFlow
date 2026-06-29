using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Orders;

public class TableSessionDao : ITableSessionDao
{
    private readonly AppDbContext _dbContext;

    public TableSessionDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DiningTable>> GetDiningTablesAsync(
        bool activeOnly,
        string? status,
        string? area,
        CancellationToken cancellationToken = default)
    {
        IQueryable<DiningTable> query = _dbContext.DiningTables
            .Include(x => x.AreaEntity)
            .Include(x => x.TableSessions);

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            string normalizedStatus = status.Trim();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(area))
        {
            string normalizedArea = area.Trim();
            query = query.Where(x => x.Area == normalizedArea || (x.AreaEntity != null && x.AreaEntity.AreaName == normalizedArea));
        }

        return await query
            .OrderBy(x => x.AreaEntity != null ? x.AreaEntity.DisplayOrder : int.MaxValue)
            .ThenBy(x => x.AreaEntity != null ? x.AreaEntity.AreaName : x.Area)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.TableName)
            .ToListAsync(cancellationToken);
    }

    public Task<DiningTable?> GetActiveTableByIdAsync(int tableId, CancellationToken cancellationToken = default)
    {
        return _dbContext.DiningTables
            .FirstOrDefaultAsync(x => x.TableId == tableId && x.IsActive, cancellationToken);
    }

    public Task<DiningTable?> GetActiveTableByQrTokenAsync(string qrToken, CancellationToken cancellationToken = default)
    {
        return _dbContext.DiningTables
            .FirstOrDefaultAsync(x => x.QrToken == qrToken && x.IsActive, cancellationToken);
    }

    public Task<DiningTable?> GetTableByIdAsync(int tableId, CancellationToken cancellationToken = default)
    {
        return _dbContext.DiningTables.FirstOrDefaultAsync(x => x.TableId == tableId, cancellationToken);
    }

    public async Task<IReadOnlyList<TableSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TableSessions
            .Where(x => x.Status == "Open" || x.Status == "WaitingPayment")
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<TableSessionCustomer?> GetSessionCustomerAsync(
        int tableSessionId,
        string clientToken,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TableSessionCustomers
            .Include(x => x.TableSession)
            .ThenInclude(x => x!.Table)
            .FirstOrDefaultAsync(x => x.TableSessionId == tableSessionId && x.ClientToken == clientToken, cancellationToken);
    }

    public Task<TableSessionCustomer?> GetSessionCustomerByTokenAsync(
        string clientToken,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TableSessionCustomers
            .Include(x => x.TableSession)
            .ThenInclude(x => x!.Table)
            .FirstOrDefaultAsync(x => x.ClientToken == clientToken, cancellationToken);
    }

    public Task<TableSession?> GetByIdAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TableSessions.FirstOrDefaultAsync(x => x.TableSessionId == tableSessionId, cancellationToken);
    }

    public Task<TableSession?> GetCurrentByTableIdAsync(int tableId, CancellationToken cancellationToken = default)
    {
        return _dbContext.TableSessions
            .FirstOrDefaultAsync(x => x.TableId == tableId && (x.Status == "Open" || x.Status == "WaitingPayment"), cancellationToken);
    }

    public Task<TableSession?> GetCurrentCustomerSessionByTableIdAsync(
        int tableId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TableSessions
            .FirstOrDefaultAsync(
                x => x.TableId == tableId &&
                     (x.Status == "Browsing" || x.Status == "Open" || x.Status == "WaitingPayment"),
                cancellationToken);
    }

    public async Task<IReadOnlyList<TableSession>> GetExpiredBrowsingSessionsAsync(
        DateTime expiresBefore,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TableSessions
            .Include(x => x.Orders)
            .Include(x => x.ServiceRequests)
            .Where(x =>
                x.Status == "Browsing" &&
                x.StartedAt <= expiresBefore &&
                !x.Orders.Any())
            .ToListAsync(cancellationToken);
    }

    public async Task AddTableSessionAsync(TableSession tableSession, CancellationToken cancellationToken = default)
    {
        await _dbContext.TableSessions.AddAsync(tableSession, cancellationToken);
    }

    public async Task AddSessionCustomerAsync(TableSessionCustomer sessionCustomer, CancellationToken cancellationToken = default)
    {
        await _dbContext.TableSessionCustomers.AddAsync(sessionCustomer, cancellationToken);
    }
}
