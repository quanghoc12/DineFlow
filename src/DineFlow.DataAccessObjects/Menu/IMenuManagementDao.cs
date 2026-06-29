using DineFlow.BusinessObjects.Menu;

namespace DineFlow.DataAccessObjects.Menu;

public interface IMenuManagementDao
{
    Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<bool> CategoryNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task<MenuItem?> GetItemAsync(int itemId, CancellationToken cancellationToken = default);
    Task<bool> ItemNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddItemAsync(MenuItem item, CancellationToken cancellationToken = default);
    Task<List<ChoiceGroup>> GetChoiceGroupsAsync(CancellationToken cancellationToken = default);
    Task<ChoiceGroup?> GetChoiceGroupAsync(int choiceGroupId, CancellationToken cancellationToken = default);
    Task<ChoiceItem?> GetChoiceItemAsync(int choiceItemId, CancellationToken cancellationToken = default);
    Task<bool> ChoiceGroupNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default);
    Task<bool> ChoiceItemNameExistsAsync(int groupId, string name, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddChoiceGroupAsync(ChoiceGroup group, CancellationToken cancellationToken = default);
    Task AddChoiceItemAsync(ChoiceItem item, CancellationToken cancellationToken = default);
    Task<MenuItemChoiceGroup?> GetAssignmentAsync(int menuItemId, int choiceGroupId, CancellationToken cancellationToken = default);
    Task AddAssignmentAsync(MenuItemChoiceGroup assignment, CancellationToken cancellationToken = default);
    void RemoveAssignment(MenuItemChoiceGroup assignment);
    Task<List<SalesChannel>> GetSalesChannelsAsync(CancellationToken cancellationToken = default);
    Task<MenuItemChannelPrice?> GetMenuItemChannelPriceAsync(int menuItemId, int salesChannelId, CancellationToken cancellationToken = default);
    Task AddMenuItemChannelPriceAsync(MenuItemChannelPrice price, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
