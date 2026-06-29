using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Tables;

public sealed class TableManagementDao : ITableManagementDao
{
    private readonly AppDbContext _dbContext;

    public TableManagementDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<DiningTable>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.DiningTables
            .AsNoTracking()
            .OrderBy(table => table.Area)
            .ThenBy(table => table.TableName)
            .ToListAsync(cancellationToken);

    public Task<DiningTable?> GetByIdAsync(int tableId, CancellationToken cancellationToken = default) =>
        _dbContext.DiningTables.FirstOrDefaultAsync(table => table.TableId == tableId, cancellationToken);

    public Task<bool> NameExistsInAreaAsync(
        string name,
        string area,
        int? excludedTableId = null,
        CancellationToken cancellationToken = default)
    {
        string normalizedName = name.Trim().ToLower();
        string normalizedArea = area.Trim().ToLower();
        return _dbContext.DiningTables.AnyAsync(
            table => table.TableName.ToLower() == normalizedName &&
                     table.Area.ToLower() == normalizedArea &&
                     (!excludedTableId.HasValue || table.TableId != excludedTableId.Value),
            cancellationToken);
    }

    public Task<bool> QrTokenExistsAsync(string token, CancellationToken cancellationToken = default) =>
        _dbContext.DiningTables.AnyAsync(table => table.QrToken == token, cancellationToken);

    public Task AddAsync(DiningTable table, CancellationToken cancellationToken = default) =>
        _dbContext.DiningTables.AddAsync(table, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
