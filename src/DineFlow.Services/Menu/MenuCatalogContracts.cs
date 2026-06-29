namespace DineFlow.Services.Menu;

public class MenuCatalogDto
{
    public IReadOnlyList<MenuCategoryDto> Categories { get; set; } = [];
    public IReadOnlyList<MenuCatalogItemDto> Items { get; set; } = [];
}

public class MenuCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class MenuCatalogItemDto
{
    public int MenuItemId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public decimal ChannelExtraPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public int? Stock { get; set; }
    public IReadOnlyList<MenuItemChoiceGroupDto> ChoiceGroups { get; set; } = [];
}

public class MenuItemChoiceGroupDto
{
    public int ChoiceGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int MaxSelectDefault { get; set; }
    public int? MaxSelectOverride { get; set; }
    public int EffectiveMaxSelect { get; set; }
    public int DisplayOrder { get; set; }
    public IReadOnlyList<MenuChoiceItemDto> ChoiceItems { get; set; } = [];
}

public class MenuChoiceItemDto
{
    public int ChoiceItemId { get; set; }
    public string ChoiceName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public decimal ChannelExtraPrice { get; set; }
    public decimal FinalExtraPrice { get; set; }
    public bool IsAvailable { get; set; }
}

public class MenuCatalogFilter
{
    public int? CategoryId { get; set; }
    public string? Search { get; set; }
    public string? SalesChannelCode { get; set; }
    public bool AvailableOnly { get; set; } = true;
}

public interface IMenuCatalogService
{
    Task<MenuCatalogDto> GetCatalogAsync(MenuCatalogFilter filter, CancellationToken cancellationToken = default);
    Task<MenuCatalogItemDto?> GetMenuItemAsync(
        int menuItemId,
        string? salesChannelCode = null,
        CancellationToken cancellationToken = default);
}
