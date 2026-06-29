using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public interface IMenuReadRepository
{
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<SalesChannel?> GetSalesChannelByCodeAsync(string channelCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItem>> GetCatalogItemsAsync(
        bool availableOnly,
        int? categoryId,
        string? search,
        CancellationToken cancellationToken = default);
    Task<MenuItem?> GetCatalogItemByIdAsync(int menuItemId, CancellationToken cancellationToken = default);
    Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItemChoiceGroup>> GetChoiceGroupAssignmentsByMenuItemIdAsync(
        int menuItemId,
        CancellationToken cancellationToken = default);
    Task<ChoiceGroup?> GetAvailableChoiceGroupByIdAsync(int choiceGroupId, CancellationToken cancellationToken = default);
    Task<ChoiceItem?> GetAvailableChoiceItemAsync(
        int choiceGroupId,
        int choiceItemId,
        CancellationToken cancellationToken = default);
    Task<decimal> GetMenuItemChannelExtraPriceAsync(
        int menuItemId,
        int salesChannelId,
        CancellationToken cancellationToken = default);
    Task<decimal> GetChoiceItemChannelExtraPriceAsync(
        int choiceItemId,
        int salesChannelId,
        CancellationToken cancellationToken = default);
}
