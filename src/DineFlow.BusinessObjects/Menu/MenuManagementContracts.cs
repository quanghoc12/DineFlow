namespace DineFlow.BusinessObjects.Menu;

public sealed class ManagedCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ManagedMenuItemDto
{
    public int MenuItemId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public int? Stock { get; set; }
    public List<ManagedMenuItemChoiceGroupDto> ChoiceGroups { get; set; } = [];
    public List<ManagedChannelPriceDto> ChannelPrices { get; set; } = [];
}

public sealed class ManagedMenuItemChoiceGroupDto
{
    public int ChoiceGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int? MaxSelect { get; set; }
    public int EffectiveMaxSelect { get; set; }
    public bool IsRequired { get; set; }
    public bool IsAvailable { get; set; }
}

public sealed class SaveCategoryRequest
{
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class SaveMenuItemRequest
{
    public int? MenuItemId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public int? Stock { get; set; }
    public bool IsAvailable { get; set; }
}

public sealed class ManagedChoiceGroupDto
{
    public int ChoiceGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public bool IsRequired { get; set; }
    public int MaxSelectDefault { get; set; }
    public List<ManagedChoiceItemDto> Items { get; set; } = [];
}

public sealed class ManagedChoiceItemDto
{
    public int ChoiceItemId { get; set; }
    public string ChoiceName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
    public bool IsAvailable { get; set; }
    public List<ManagedChannelPriceDto> ChannelPrices { get; set; } = [];
}

public sealed class SaveChoiceGroupRequest
{
    public int? ChoiceGroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int MaxSelectDefault { get; set; } = 1;
}

public sealed class SaveChoiceItemRequest
{
    public int? ChoiceItemId { get; set; }
    public int ChoiceGroupId { get; set; }
    public string ChoiceName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
}

public sealed class AssignChoiceGroupRequest
{
    public int MenuItemId { get; set; }
    public int ChoiceGroupId { get; set; }
    public int DisplayOrder { get; set; }
    public int? MaxSelect { get; set; }
}

public sealed class SaveChannelPriceRequest
{
    public int MenuItemId { get; set; }
    public int? ChoiceItemId { get; set; }
    public int SalesChannelId { get; set; }
    public decimal ChannelExtraPrice { get; set; }
}

public sealed class SaveSalesChannelRequest
{
    public int? SalesChannelId { get; set; }
    public string ChannelCode { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
}

public sealed class ManagedSalesChannelDto
{
    public int SalesChannelId { get; set; }
    public string ChannelCode { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public sealed class ManagedChannelPriceDto
{
    public int SalesChannelId { get; set; }
    public string ChannelCode { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public decimal ChannelExtraPrice { get; set; }
}
