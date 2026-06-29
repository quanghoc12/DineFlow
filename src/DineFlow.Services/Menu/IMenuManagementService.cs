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
    Task<IReadOnlyList<ManagedChoiceGroupDto>> GetChoiceGroupsAsync(CancellationToken cancellationToken = default);
    Task SaveChoiceGroupAsync(SaveChoiceGroupRequest request, CancellationToken cancellationToken = default);
    Task SaveChoiceItemAsync(SaveChoiceItemRequest request, CancellationToken cancellationToken = default);
    Task SetChoiceGroupAvailabilityAsync(int choiceGroupId, bool available, CancellationToken cancellationToken = default);
    Task SetChoiceItemAvailabilityAsync(int choiceItemId, bool available, CancellationToken cancellationToken = default);
    Task AssignChoiceGroupAsync(AssignChoiceGroupRequest request, CancellationToken cancellationToken = default);
    Task RemoveChoiceGroupAssignmentAsync(int menuItemId, int choiceGroupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManagedSalesChannelDto>> GetSalesChannelsAsync(CancellationToken cancellationToken = default);
    Task SaveSalesChannelAsync(SaveSalesChannelRequest request, CancellationToken cancellationToken = default);
    Task SetSalesChannelActiveAsync(int salesChannelId, bool active, CancellationToken cancellationToken = default);
    Task SaveMenuItemChannelPriceAsync(SaveChannelPriceRequest request, CancellationToken cancellationToken = default);
    Task SaveChoiceItemChannelPriceAsync(SaveChannelPriceRequest request, CancellationToken cancellationToken = default);
}
