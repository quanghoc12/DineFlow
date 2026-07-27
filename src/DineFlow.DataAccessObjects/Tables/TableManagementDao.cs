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
            .Include(table => table.AreaEntity)
            .Include(table => table.TableSessions)
            .OrderBy(table => table.AreaEntity != null ? table.AreaEntity.DisplayOrder : int.MaxValue)
            .ThenBy(table => table.Area)
            .ThenBy(table => table.DisplayOrder)
            .ThenBy(table => table.TableName)
            .ToListAsync(cancellationToken);

    public Task<List<DiningTable>> GetAllForUpdateAsync(CancellationToken cancellationToken = default) =>
        _dbContext.DiningTables
            .Include(table => table.AreaEntity)
            .Include(table => table.TableSessions)
            .OrderBy(table => table.DisplayOrder)
            .ThenBy(table => table.TableName)
            .ToListAsync(cancellationToken);

    public Task<List<Area>> GetAreasAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Areas
            .AsNoTracking()
            .Include(area => area.DiningTables)
            .OrderBy(area => area.DisplayOrder)
            .ThenBy(area => area.AreaName)
            .ToListAsync(cancellationToken);

    public Task<List<Area>> GetAreasForUpdateAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Areas
            .OrderBy(area => area.DisplayOrder)
            .ThenBy(area => area.AreaName)
            .ToListAsync(cancellationToken);

    public Task<Area?> GetAreaAsync(int areaId, CancellationToken cancellationToken = default) =>
        _dbContext.Areas.FirstOrDefaultAsync(area => area.AreaId == areaId, cancellationToken);

    public Task<Area?> GetAreaByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        string normalized = name.Trim().ToLower();
        return _dbContext.Areas.FirstOrDefaultAsync(
            area => area.AreaName.ToLower() == normalized,
            cancellationToken);
    }

    public Task<bool> AreaNameExistsAsync(
        string name,
        int? excludedAreaId = null,
        CancellationToken cancellationToken = default)
    {
        string normalized = name.Trim().ToLower();
        return _dbContext.Areas.AnyAsync(
            area => area.AreaName.ToLower() == normalized &&
                    (!excludedAreaId.HasValue || area.AreaId != excludedAreaId.Value),
            cancellationToken);
    }

    public Task AddAreaAsync(Area area, CancellationToken cancellationToken = default) =>
        _dbContext.Areas.AddAsync(area, cancellationToken).AsTask();

    public Task<DiningTable?> GetByIdAsync(int tableId, CancellationToken cancellationToken = default) =>
        _dbContext.DiningTables
            .Include(table => table.AreaEntity)
            .Include(table => table.TableSessions)
            .FirstOrDefaultAsync(table => table.TableId == tableId, cancellationToken);

    public Task<bool> NameExistsInAreaAsync(
        string name,
        int? areaId,
        string area,
        int? excludedTableId = null,
        CancellationToken cancellationToken = default)
    {
        string normalizedName = name.Trim().ToLower();
        string normalizedArea = area.Trim().ToLower();
        return _dbContext.DiningTables.AnyAsync(
            table => table.TableName.ToLower() == normalizedName &&
                     (areaId.HasValue
                         ? table.AreaId == areaId
                         : table.AreaId == null && table.Area.ToLower() == normalizedArea) &&
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
