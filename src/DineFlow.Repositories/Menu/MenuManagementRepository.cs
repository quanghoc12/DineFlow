using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public sealed class MenuManagementRepository : IMenuManagementRepository
{
    private readonly IMenuManagementDao _dao;

    public MenuManagementRepository(IMenuManagementDao dao) => _dao = dao;

    public Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default) => _dao.GetCategoriesAsync(cancellationToken);
    public Task<Category?> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default) => _dao.GetCategoryAsync(categoryId, cancellationToken);
    public Task<bool> CategoryNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default) => _dao.CategoryNameExistsAsync(name, excludedId, cancellationToken);
    public Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default) => _dao.AddCategoryAsync(category, cancellationToken);
    public Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken = default) => _dao.GetItemsAsync(cancellationToken);
    public Task<MenuItem?> GetItemAsync(int itemId, CancellationToken cancellationToken = default) => _dao.GetItemAsync(itemId, cancellationToken);
    public Task<bool> ItemNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default) => _dao.ItemNameExistsAsync(name, excludedId, cancellationToken);
    public Task AddItemAsync(MenuItem item, CancellationToken cancellationToken = default) => _dao.AddItemAsync(item, cancellationToken);
    public Task<List<ChoiceGroup>> GetChoiceGroupsAsync(CancellationToken cancellationToken = default) => _dao.GetChoiceGroupsAsync(cancellationToken);
    public Task<ChoiceGroup?> GetChoiceGroupAsync(int choiceGroupId, CancellationToken cancellationToken = default) => _dao.GetChoiceGroupAsync(choiceGroupId, cancellationToken);
    public Task<ChoiceItem?> GetChoiceItemAsync(int choiceItemId, CancellationToken cancellationToken = default) => _dao.GetChoiceItemAsync(choiceItemId, cancellationToken);
    public Task<bool> ChoiceGroupNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default) => _dao.ChoiceGroupNameExistsAsync(name, excludedId, cancellationToken);
    public Task<bool> ChoiceItemNameExistsAsync(int groupId, string name, int? excludedId = null, CancellationToken cancellationToken = default) => _dao.ChoiceItemNameExistsAsync(groupId, name, excludedId, cancellationToken);
    public Task AddChoiceGroupAsync(ChoiceGroup group, CancellationToken cancellationToken = default) => _dao.AddChoiceGroupAsync(group, cancellationToken);
    public Task AddChoiceItemAsync(ChoiceItem item, CancellationToken cancellationToken = default) => _dao.AddChoiceItemAsync(item, cancellationToken);
    public Task<MenuItemChoiceGroup?> GetAssignmentAsync(int menuItemId, int choiceGroupId, CancellationToken cancellationToken = default) => _dao.GetAssignmentAsync(menuItemId, choiceGroupId, cancellationToken);
    public Task AddAssignmentAsync(MenuItemChoiceGroup assignment, CancellationToken cancellationToken = default) => _dao.AddAssignmentAsync(assignment, cancellationToken);
    public void RemoveAssignment(MenuItemChoiceGroup assignment) => _dao.RemoveAssignment(assignment);
    public Task<List<SalesChannel>> GetSalesChannelsAsync(CancellationToken cancellationToken = default) => _dao.GetSalesChannelsAsync(cancellationToken);
    public Task<MenuItemChannelPrice?> GetMenuItemChannelPriceAsync(int menuItemId, int salesChannelId, CancellationToken cancellationToken = default) => _dao.GetMenuItemChannelPriceAsync(menuItemId, salesChannelId, cancellationToken);
    public Task AddMenuItemChannelPriceAsync(MenuItemChannelPrice price, CancellationToken cancellationToken = default) => _dao.AddMenuItemChannelPriceAsync(price, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dao.SaveChangesAsync(cancellationToken);
}
