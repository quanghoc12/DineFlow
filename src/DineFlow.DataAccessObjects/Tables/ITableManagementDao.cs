using DineFlow.BusinessObjects.Tables;

namespace DineFlow.DataAccessObjects.Tables;

public interface ITableManagementDao
{
    Task<List<DiningTable>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DiningTable?> GetByIdAsync(int tableId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsInAreaAsync(string name, string area, int? excludedTableId = null, CancellationToken cancellationToken = default);
    Task<bool> QrTokenExistsAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(DiningTable table, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
