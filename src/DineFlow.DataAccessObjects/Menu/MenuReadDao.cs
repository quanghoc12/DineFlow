using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Menu;

public class MenuReadDao : IMenuReadDao
{
    private readonly AppDbContext _dbContext;

    public MenuReadDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CategoryName)
            .ToListAsync(cancellationToken);
    }

    public Task<SalesChannel?> GetSalesChannelByCodeAsync(string channelCode, CancellationToken cancellationToken = default)
    {
        string normalizedCode = channelCode.Trim().ToUpper();

        return _dbContext.SalesChannels
            .FirstOrDefaultAsync(x => x.ChannelCode == normalizedCode && x.IsActive && !x.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetCatalogItemsAsync(
        bool availableOnly,
        int? categoryId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        IQueryable<MenuItem> query = _dbContext.MenuItems
            .Include(x => x.Category)
            .Include(x => x.ChannelPrices)
            .Include(x => x.MenuItemChoiceGroups)
                .ThenInclude(x => x.ChoiceGroup)
                    .ThenInclude(x => x!.ChoiceItems)
                        .ThenInclude(x => x.ChannelPrices)
            .AsSplitQuery()
            .Where(x => x.Category != null && x.Category.IsActive && !x.IsDeleted);

        if (availableOnly)
        {
            query = query.Where(x => x.IsAvailable);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string keyword = search.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)));
        }

        return await query
            .OrderBy(x => x.Category!.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<MenuItem?> GetCatalogItemByIdAsync(int menuItemId, CancellationToken cancellationToken = default)
    {
        return _dbContext.MenuItems
            .Include(x => x.Category)
            .Include(x => x.ChannelPrices)
            .Include(x => x.MenuItemChoiceGroups)
                .ThenInclude(x => x.ChoiceGroup)
                    .ThenInclude(x => x!.ChoiceItems)
                        .ThenInclude(x => x.ChannelPrices)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.MenuItemId == menuItemId && x.Category != null && x.Category.IsActive && !x.IsDeleted, cancellationToken);
    }

    public Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId, CancellationToken cancellationToken = default)
    {
        return _dbContext.MenuItems.FirstOrDefaultAsync(x => x.MenuItemId == menuItemId, cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItemChoiceGroup>> GetChoiceGroupAssignmentsByMenuItemIdAsync(
        int menuItemId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MenuItemChoiceGroups
            .Where(x => x.MenuItemId == menuItemId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public Task<ChoiceGroup?> GetAvailableChoiceGroupByIdAsync(
        int choiceGroupId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ChoiceGroups
            .FirstOrDefaultAsync(x => x.ChoiceGroupId == choiceGroupId && x.IsAvailable, cancellationToken);
    }

    public Task<ChoiceItem?> GetAvailableChoiceItemAsync(
        int choiceGroupId,
        int choiceItemId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.ChoiceItems.FirstOrDefaultAsync(x =>
            x.ChoiceGroupId == choiceGroupId &&
            x.ChoiceItemId == choiceItemId &&
            x.IsAvailable,
            cancellationToken);
    }

    public async Task<decimal> GetMenuItemChannelExtraPriceAsync(
        int menuItemId,
        int salesChannelId,
        CancellationToken cancellationToken = default)
    {
        decimal? extraPrice = await _dbContext.MenuItemChannelPrices
            .Where(x => x.MenuItemId == menuItemId && x.SalesChannelId == salesChannelId)
            .Select(x => (decimal?)x.ChannelExtraPrice)
            .FirstOrDefaultAsync(cancellationToken);

        return extraPrice ?? 0m;
    }

    public async Task<decimal> GetChoiceItemChannelExtraPriceAsync(
        int choiceItemId,
        int salesChannelId,
        CancellationToken cancellationToken = default)
    {
        decimal? extraPrice = await _dbContext.ChoiceItemChannelPrices
            .Where(x => x.ChoiceItemId == choiceItemId && x.SalesChannelId == salesChannelId)
            .Select(x => (decimal?)x.ChannelExtraPrice)
            .FirstOrDefaultAsync(cancellationToken);

        return extraPrice ?? 0m;
    }
}
