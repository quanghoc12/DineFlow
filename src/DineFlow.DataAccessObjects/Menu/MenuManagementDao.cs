using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Menu;

public sealed class MenuManagementDao : IMenuManagementDao
{
    private readonly AppDbContext _dbContext;

    public MenuManagementDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Categories
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.CategoryName)
            .ToListAsync(cancellationToken);

    public Task<Category?> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default) =>
        _dbContext.Categories.FirstOrDefaultAsync(category => category.CategoryId == categoryId, cancellationToken);

    public Task<bool> CategoryNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default)
    {
        string normalized = name.Trim().ToLower();
        return _dbContext.Categories.AnyAsync(
            category => category.CategoryName.ToLower() == normalized &&
                        (!excludedId.HasValue || category.CategoryId != excludedId.Value),
            cancellationToken);
    }

    public Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default) =>
        _dbContext.Categories.AddAsync(category, cancellationToken).AsTask();

    public Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken = default) =>
        _dbContext.MenuItems.AsNoTracking()
            .Where(item => !item.IsDeleted)
            .Include(item => item.Category)
            .Include(item => item.MenuItemChoiceGroups)
                .ThenInclude(assignment => assignment.ChoiceGroup)
            .Include(item => item.ChannelPrices)
                .ThenInclude(price => price.SalesChannel)
            .OrderBy(item => item.Category!.DisplayOrder)
            .ThenBy(item => item.Name)
            .ToListAsync(cancellationToken);

    public Task<MenuItem?> GetItemAsync(int itemId, CancellationToken cancellationToken = default) =>
        _dbContext.MenuItems.FirstOrDefaultAsync(item => item.MenuItemId == itemId, cancellationToken);

    public Task<bool> ItemNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default)
    {
        string normalized = name.Trim().ToLower();
        return _dbContext.MenuItems.AnyAsync(
            item => item.Name.ToLower() == normalized &&
                    !item.IsDeleted &&
                    (!excludedId.HasValue || item.MenuItemId != excludedId.Value),
            cancellationToken);
    }

    public Task AddItemAsync(MenuItem item, CancellationToken cancellationToken = default) =>
        _dbContext.MenuItems.AddAsync(item, cancellationToken).AsTask();

    public Task<List<ChoiceGroup>> GetChoiceGroupsAsync(CancellationToken cancellationToken = default) =>
        _dbContext.ChoiceGroups.AsNoTracking()
            .Include(group => group.ChoiceItems)
                .ThenInclude(item => item.ChannelPrices)
                    .ThenInclude(price => price.SalesChannel)
            .OrderBy(group => group.GroupName)
            .ToListAsync(cancellationToken);

    public Task<ChoiceGroup?> GetChoiceGroupAsync(int choiceGroupId, CancellationToken cancellationToken = default) =>
        _dbContext.ChoiceGroups.FirstOrDefaultAsync(group => group.ChoiceGroupId == choiceGroupId, cancellationToken);

    public Task<ChoiceItem?> GetChoiceItemAsync(int choiceItemId, CancellationToken cancellationToken = default) =>
        _dbContext.ChoiceItems.FirstOrDefaultAsync(item => item.ChoiceItemId == choiceItemId, cancellationToken);

    public Task<bool> ChoiceGroupNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default)
    {
        string normalized = name.Trim().ToLower();
        return _dbContext.ChoiceGroups.AnyAsync(
            group => group.GroupName.ToLower() == normalized &&
                     (!excludedId.HasValue || group.ChoiceGroupId != excludedId),
            cancellationToken);
    }

    public Task<bool> ChoiceItemNameExistsAsync(
        int groupId, string name, int? excludedId = null, CancellationToken cancellationToken = default)
    {
        string normalized = name.Trim().ToLower();
        return _dbContext.ChoiceItems.AnyAsync(
            item => item.ChoiceGroupId == groupId &&
                    item.ChoiceName.ToLower() == normalized &&
                    (!excludedId.HasValue || item.ChoiceItemId != excludedId),
            cancellationToken);
    }

    public Task AddChoiceGroupAsync(ChoiceGroup group, CancellationToken cancellationToken = default) =>
        _dbContext.ChoiceGroups.AddAsync(group, cancellationToken).AsTask();

    public Task AddChoiceItemAsync(ChoiceItem item, CancellationToken cancellationToken = default) =>
        _dbContext.ChoiceItems.AddAsync(item, cancellationToken).AsTask();

    public Task<MenuItemChoiceGroup?> GetAssignmentAsync(
        int menuItemId, int choiceGroupId, CancellationToken cancellationToken = default) =>
        _dbContext.MenuItemChoiceGroups.FirstOrDefaultAsync(
            assignment => assignment.MenuItemId == menuItemId && assignment.ChoiceGroupId == choiceGroupId,
            cancellationToken);

    public Task AddAssignmentAsync(MenuItemChoiceGroup assignment, CancellationToken cancellationToken = default) =>
        _dbContext.MenuItemChoiceGroups.AddAsync(assignment, cancellationToken).AsTask();

    public void RemoveAssignment(MenuItemChoiceGroup assignment) => _dbContext.MenuItemChoiceGroups.Remove(assignment);

    public void RemoveCategory(Category category) => _dbContext.Categories.Remove(category);

    public Task<List<SalesChannel>> GetSalesChannelsAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SalesChannels.AsNoTracking().Where(channel => !channel.IsDeleted).OrderBy(channel => channel.ChannelName).ToListAsync(cancellationToken);

    public Task<MenuItemChannelPrice?> GetMenuItemChannelPriceAsync(
        int menuItemId, int salesChannelId, CancellationToken cancellationToken = default) =>
        _dbContext.MenuItemChannelPrices.FirstOrDefaultAsync(
            price => price.MenuItemId == menuItemId && price.SalesChannelId == salesChannelId,
            cancellationToken);

    public Task AddMenuItemChannelPriceAsync(MenuItemChannelPrice price, CancellationToken cancellationToken = default) =>
        _dbContext.MenuItemChannelPrices.AddAsync(price, cancellationToken).AsTask();

    public Task<SalesChannel?> GetSalesChannelAsync(int salesChannelId, CancellationToken cancellationToken = default) =>
        _dbContext.SalesChannels.FirstOrDefaultAsync(channel => channel.SalesChannelId == salesChannelId, cancellationToken);

    public Task<bool> SalesChannelCodeExistsAsync(string code, int? excludedId = null, CancellationToken cancellationToken = default)
    {
        string normalized = code.Trim().ToUpper();
        return _dbContext.SalesChannels.AnyAsync(
            channel => !channel.IsDeleted && channel.ChannelCode.ToUpper() == normalized &&
                       (!excludedId.HasValue || channel.SalesChannelId != excludedId.Value),
            cancellationToken);
    }

    public Task AddSalesChannelAsync(SalesChannel channel, CancellationToken cancellationToken = default) =>
        _dbContext.SalesChannels.AddAsync(channel, cancellationToken).AsTask();

    public Task<ChoiceItemChannelPrice?> GetChoiceItemChannelPriceAsync(
        int choiceItemId, int salesChannelId, CancellationToken cancellationToken = default) =>
        _dbContext.ChoiceItemChannelPrices.FirstOrDefaultAsync(
            price => price.ChoiceItemId == choiceItemId && price.SalesChannelId == salesChannelId,
            cancellationToken);

    public Task AddChoiceItemChannelPriceAsync(ChoiceItemChannelPrice price, CancellationToken cancellationToken = default) =>
        _dbContext.ChoiceItemChannelPrices.AddAsync(price, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
