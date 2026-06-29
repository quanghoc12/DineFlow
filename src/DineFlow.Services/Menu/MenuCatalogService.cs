using DineFlow.BusinessObjects.Menu;
using DineFlow.Repositories.Menu;

namespace DineFlow.Services.Menu;

public class MenuCatalogService : IMenuCatalogService
{
    private readonly IMenuReadRepository _menuReadRepository;

    public MenuCatalogService(IMenuReadRepository menuReadRepository)
    {
        _menuReadRepository = menuReadRepository;
    }

    public async Task<MenuCatalogDto> GetCatalogAsync(
        MenuCatalogFilter filter,
        CancellationToken cancellationToken = default)
    {
        SalesChannel? salesChannel = await ResolveSalesChannelAsync(filter.SalesChannelCode, cancellationToken);

        List<MenuCategoryDto> categories = (await _menuReadRepository.GetActiveCategoriesAsync(cancellationToken))
            .Select(x => new MenuCategoryDto
            {
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                DisplayOrder = x.DisplayOrder
            })
            .ToList();

        List<MenuCatalogItemDto> items = (await _menuReadRepository.GetCatalogItemsAsync(
                filter.AvailableOnly,
                filter.CategoryId,
                filter.Search,
                cancellationToken))
            .Select(item => MapItem(item, salesChannel?.SalesChannelId))
            .ToList();

        return new MenuCatalogDto
        {
            Categories = categories,
            Items = items
        };
    }

    public async Task<MenuCatalogItemDto?> GetMenuItemAsync(
        int menuItemId,
        string? salesChannelCode = null,
        CancellationToken cancellationToken = default)
    {
        SalesChannel? salesChannel = await ResolveSalesChannelAsync(salesChannelCode, cancellationToken);
        MenuItem? item = await _menuReadRepository.GetCatalogItemByIdAsync(menuItemId, cancellationToken);

        return item is null ? null : MapItem(item, salesChannel?.SalesChannelId);
    }

    private async Task<SalesChannel?> ResolveSalesChannelAsync(string? channelCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(channelCode))
        {
            return null;
        }

        return await _menuReadRepository.GetSalesChannelByCodeAsync(channelCode, cancellationToken);
    }

    private static MenuCatalogItemDto MapItem(MenuItem item, int? salesChannelId)
    {
        decimal channelExtraPrice = GetMenuItemChannelExtraPrice(item, salesChannelId);

        return new MenuCatalogItemDto
        {
            MenuItemId = item.MenuItemId,
            CategoryId = item.CategoryId,
            Name = item.Name,
            Description = item.Description,
            BasePrice = item.BasePrice,
            ChannelExtraPrice = channelExtraPrice,
            FinalPrice = item.BasePrice + channelExtraPrice,
            ImageUrl = item.ImageUrl,
            IsAvailable = item.IsAvailable,
            Stock = item.Stock,
            IsOutOfStock = item.IsOutOfStock || item.Stock == 0,
            ChoiceGroups = item.MenuItemChoiceGroups
                .Where(x => x.ChoiceGroup != null && x.ChoiceGroup.IsAvailable)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ChoiceGroup!.GroupName)
                .Select(assignment => MapChoiceGroup(assignment, salesChannelId))
                .ToList()
        };
    }

    private static MenuItemChoiceGroupDto MapChoiceGroup(MenuItemChoiceGroup assignment, int? salesChannelId)
    {
        ChoiceGroup group = assignment.ChoiceGroup!;
        int effectiveMaxSelect = assignment.MaxSelect ?? group.MaxSelectDefault;

        return new MenuItemChoiceGroupDto
        {
            ChoiceGroupId = group.ChoiceGroupId,
            GroupName = group.GroupName,
            IsRequired = group.IsRequired,
            MaxSelectDefault = group.MaxSelectDefault,
            MaxSelectOverride = assignment.MaxSelect,
            EffectiveMaxSelect = effectiveMaxSelect,
            DisplayOrder = assignment.DisplayOrder,
            ChoiceItems = group.ChoiceItems
                .Where(x => x.IsAvailable)
                .OrderBy(x => x.ChoiceName)
                .Select(x =>
                {
                    decimal channelExtraPrice = GetChoiceItemChannelExtraPrice(x, salesChannelId);

                    return new MenuChoiceItemDto
                    {
                        ChoiceItemId = x.ChoiceItemId,
                        ChoiceName = x.ChoiceName,
                        ExtraPrice = x.ExtraPrice,
                        ChannelExtraPrice = channelExtraPrice,
                        FinalExtraPrice = x.ExtraPrice + channelExtraPrice,
                        IsAvailable = x.IsAvailable
                    };
                })
                .ToList()
        };
    }

    private static decimal GetMenuItemChannelExtraPrice(MenuItem item, int? salesChannelId)
    {
        if (salesChannelId is null)
        {
            return 0m;
        }

        return item.ChannelPrices
            .FirstOrDefault(x => x.SalesChannelId == salesChannelId.Value)
            ?.ChannelExtraPrice ?? 0m;
    }

    private static decimal GetChoiceItemChannelExtraPrice(ChoiceItem item, int? salesChannelId)
    {
        if (salesChannelId is null)
        {
            return 0m;
        }

        return item.ChannelPrices
            .FirstOrDefault(x => x.SalesChannelId == salesChannelId.Value)
            ?.ChannelExtraPrice ?? 0m;
    }
}
