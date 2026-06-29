using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu;

public interface IMenuManagementService
{
    Task<IReadOnlyList<ManagedCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManagedMenuItemDto>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task SaveCategoryAsync(SaveCategoryRequest request, CancellationToken cancellationToken = default);
    Task SetCategoryActiveAsync(int categoryId, bool active, CancellationToken cancellationToken = default);
    Task SaveItemAsync(SaveMenuItemRequest request, CancellationToken cancellationToken = default);
    Task SetItemAvailabilityAsync(int itemId, bool available, CancellationToken cancellationToken = default);
}
