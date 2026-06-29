using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Repositories.Tables;

public interface ITableManagementRepository
{
    Task<List<DiningTable>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Area>> GetAreasAsync(CancellationToken cancellationToken = default);
    Task<Area?> GetAreaAsync(int areaId, CancellationToken cancellationToken = default);
    Task<Area?> GetAreaByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> AreaNameExistsAsync(string name, int? excludedAreaId = null, CancellationToken cancellationToken = default);
    Task AddAreaAsync(Area area, CancellationToken cancellationToken = default);
    Task<DiningTable?> GetByIdAsync(int tableId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsInAreaAsync(string name, int? areaId, string area, int? excludedTableId = null, CancellationToken cancellationToken = default);
    Task<bool> QrTokenExistsAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(DiningTable table, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
