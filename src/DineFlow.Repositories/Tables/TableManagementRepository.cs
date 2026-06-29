using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.Tables;

namespace DineFlow.Repositories.Tables;

public sealed class TableManagementRepository : ITableManagementRepository
{
    private readonly ITableManagementDao _dao;

    public TableManagementRepository(ITableManagementDao dao)
    {
        _dao = dao;
    }

    public Task<List<DiningTable>> GetAllAsync(CancellationToken cancellationToken = default) => _dao.GetAllAsync(cancellationToken);
    public Task<List<Area>> GetAreasAsync(CancellationToken cancellationToken = default) => _dao.GetAreasAsync(cancellationToken);
    public Task<Area?> GetAreaAsync(int areaId, CancellationToken cancellationToken = default) => _dao.GetAreaAsync(areaId, cancellationToken);
    public Task<Area?> GetAreaByNameAsync(string name, CancellationToken cancellationToken = default) => _dao.GetAreaByNameAsync(name, cancellationToken);
    public Task<bool> AreaNameExistsAsync(string name, int? excludedAreaId = null, CancellationToken cancellationToken = default) =>
        _dao.AreaNameExistsAsync(name, excludedAreaId, cancellationToken);
    public Task AddAreaAsync(Area area, CancellationToken cancellationToken = default) => _dao.AddAreaAsync(area, cancellationToken);
    public Task<DiningTable?> GetByIdAsync(int tableId, CancellationToken cancellationToken = default) => _dao.GetByIdAsync(tableId, cancellationToken);
    public Task<bool> NameExistsInAreaAsync(string name, int? areaId, string area, int? excludedTableId = null, CancellationToken cancellationToken = default) =>
        _dao.NameExistsInAreaAsync(name, areaId, area, excludedTableId, cancellationToken);
    public Task<bool> QrTokenExistsAsync(string token, CancellationToken cancellationToken = default) => _dao.QrTokenExistsAsync(token, cancellationToken);
    public Task AddAsync(DiningTable table, CancellationToken cancellationToken = default) => _dao.AddAsync(table, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dao.SaveChangesAsync(cancellationToken);
}
