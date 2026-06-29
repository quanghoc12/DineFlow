using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public class MenuReadRepository : IMenuReadRepository
{
    private readonly IMenuReadDao _menuReadDao;

    public MenuReadRepository(IMenuReadDao menuReadDao)
    {
        _menuReadDao = menuReadDao;
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _menuReadDao.GetActiveCategoriesAsync(cancellationToken);
    }

    public Task<SalesChannel?> GetSalesChannelByCodeAsync(string channelCode, CancellationToken cancellationToken = default)
    {
        return _menuReadDao.GetSalesChannelByCodeAsync(channelCode, cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetCatalogItemsAsync(
        bool availableOnly,
        int? categoryId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        return await _menuReadDao.GetCatalogItemsAsync(availableOnly, categoryId, search, cancellationToken);
    }

    public Task<MenuItem?> GetCatalogItemByIdAsync(int menuItemId, CancellationToken cancellationToken = default)
    {
        return _menuReadDao.GetCatalogItemByIdAsync(menuItemId, cancellationToken);
    }

    public Task<MenuItem?> GetMenuItemByIdAsync(int menuItemId, CancellationToken cancellationToken = default)
    {
        return _menuReadDao.GetMenuItemByIdAsync(menuItemId, cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItemChoiceGroup>> GetChoiceGroupAssignmentsByMenuItemIdAsync(
        int menuItemId,
        CancellationToken cancellationToken = default)
    {
        return await _menuReadDao.GetChoiceGroupAssignmentsByMenuItemIdAsync(menuItemId, cancellationToken);
    }

    public Task<ChoiceGroup?> GetAvailableChoiceGroupByIdAsync(
        int choiceGroupId,
        CancellationToken cancellationToken = default)
    {
        return _menuReadDao.GetAvailableChoiceGroupByIdAsync(choiceGroupId, cancellationToken);
    }

    public Task<ChoiceItem?> GetAvailableChoiceItemAsync(
        int choiceGroupId,
        int choiceItemId,
        CancellationToken cancellationToken = default)
    {
        return _menuReadDao.GetAvailableChoiceItemAsync(choiceGroupId, choiceItemId, cancellationToken);
    }

    public Task<decimal> GetMenuItemChannelExtraPriceAsync(
        int menuItemId,
        int salesChannelId,
        CancellationToken cancellationToken = default)
    {
        return _menuReadDao.GetMenuItemChannelExtraPriceAsync(menuItemId, salesChannelId, cancellationToken);
    }

    public Task<decimal> GetChoiceItemChannelExtraPriceAsync(
        int choiceItemId,
        int salesChannelId,
        CancellationToken cancellationToken = default)
    {
        return _menuReadDao.GetChoiceItemChannelExtraPriceAsync(choiceItemId, salesChannelId, cancellationToken);
    }
}
