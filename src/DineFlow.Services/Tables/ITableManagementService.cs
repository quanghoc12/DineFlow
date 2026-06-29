using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Services.Tables;

public interface ITableManagementService
{
    Task<IReadOnlyList<ManagedTableDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManagedAreaDto>> GetAreasAsync(CancellationToken cancellationToken = default);
    Task<ManagedAreaDto> SaveAreaAsync(SaveAreaRequest request, CancellationToken cancellationToken = default);
    Task SetAreaActiveAsync(int areaId, bool active, CancellationToken cancellationToken = default);
    Task<ManagedTableDto> CreateAsync(CreateManagedTableRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateManagedTableRequest request, CancellationToken cancellationToken = default);
    Task SetActiveAsync(int tableId, bool active, CancellationToken cancellationToken = default);
    Task<ManagedTableDto> ResetQrAsync(int tableId, CancellationToken cancellationToken = default);
}
