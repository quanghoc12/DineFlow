using DineFlow.BusinessObjects.Menu;
using DineFlow.Repositories.Menu;
using DineFlow.Services.Auth;

namespace DineFlow.Services.Menu;

public sealed class MenuManagementService : IMenuManagementService
{
    private readonly IMenuManagementRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public MenuManagementService(IMenuManagementRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ManagedCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return (await _repository.GetCategoriesAsync(cancellationToken)).Select(category => new ManagedCategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        }).ToList();
    }

    public async Task<IReadOnlyList<ManagedMenuItemDto>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return (await _repository.GetItemsAsync(cancellationToken)).Select(item => new ManagedMenuItemDto
        {
            MenuItemId = item.MenuItemId,
            CategoryId = item.CategoryId,
            CategoryName = item.Category?.CategoryName ?? string.Empty,
            Name = item.Name,
            Description = item.Description,
            BasePrice = item.BasePrice,
            ImageUrl = item.ImageUrl,
            IsAvailable = item.IsAvailable,
            Stock = item.Stock,
            ChoiceGroups = item.MenuItemChoiceGroups
                .OrderBy(assignment => assignment.DisplayOrder)
                .Select(assignment => new ManagedMenuItemChoiceGroupDto
                {
                    ChoiceGroupId = assignment.ChoiceGroupId,
                    GroupName = assignment.ChoiceGroup?.GroupName ?? string.Empty,
                    DisplayOrder = assignment.DisplayOrder,
                    MaxSelect = assignment.MaxSelect,
                    EffectiveMaxSelect = assignment.MaxSelect ?? assignment.ChoiceGroup?.MaxSelectDefault ?? 1,
                    IsRequired = assignment.ChoiceGroup?.IsRequired == true,
                    IsAvailable = assignment.ChoiceGroup?.IsAvailable == true
                }).ToList(),
            ChannelPrices = item.ChannelPrices.Select(MapChannelPrice).ToList()
        }).ToList();
    }

    public async Task SaveCategoryAsync(SaveCategoryRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        string name = request.CategoryName.Trim();
        if (name.Length is < 1 or > 120)
            throw new InvalidOperationException("Tên danh mục phải từ 1 đến 120 ký tự.");
        if (request.Description?.Length > 500)
            throw new InvalidOperationException("Mô tả danh mục không vượt quá 500 ký tự.");
        if (await _repository.CategoryNameExistsAsync(name, request.CategoryId, cancellationToken))
            throw new InvalidOperationException("Tên danh mục đã tồn tại.");

        DateTime now = DateTime.UtcNow;
        if (request.CategoryId is null)
        {
            await _repository.AddCategoryAsync(new Category
            {
                CategoryName = name,
                Description = request.Description?.Trim(),
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }
        else
        {
            Category category = await _repository.GetCategoryAsync(request.CategoryId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy danh mục.");
            category.CategoryName = name;
            category.Description = request.Description?.Trim();
            category.DisplayOrder = request.DisplayOrder;
            category.UpdatedAt = now;
        }
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetCategoryActiveAsync(int categoryId, bool active, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        Category category = await _repository.GetCategoryAsync(categoryId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy danh mục.");
        category.IsActive = active;
        category.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveItemAsync(SaveMenuItemRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        string name = request.Name.Trim();
        if (name.Length is < 1 or > 150)
            throw new InvalidOperationException("Tên món phải từ 1 đến 150 ký tự.");
        if (request.BasePrice < 0)
            throw new InvalidOperationException("Giá món không được âm.");
        if (request.Stock < 0)
            throw new InvalidOperationException("Tồn kho không được âm.");
        if (request.Description?.Length > 1000)
            throw new InvalidOperationException("Mô tả món không vượt quá 1000 ký tự.");
        Category category = await _repository.GetCategoryAsync(request.CategoryId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy danh mục.");
        if (!category.IsActive)
            throw new InvalidOperationException("Không thể lưu món vào danh mục đang bị ẩn.");
        if (await _repository.ItemNameExistsAsync(name, request.MenuItemId, cancellationToken))
            throw new InvalidOperationException("Tên món đã tồn tại.");

        DateTime now = DateTime.UtcNow;
        if (request.MenuItemId is null)
        {
            await _repository.AddItemAsync(new MenuItem
            {
                CategoryId = request.CategoryId,
                Name = name,
                Description = request.Description?.Trim(),
                BasePrice = request.BasePrice,
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
                Stock = request.Stock,
                IsAvailable = request.IsAvailable && (request.Stock is null || request.Stock > 0),
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }
        else
        {
            MenuItem item = await _repository.GetItemAsync(request.MenuItemId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy món.");
            item.CategoryId = request.CategoryId;
            item.Name = name;
            item.Description = request.Description?.Trim();
            item.BasePrice = request.BasePrice;
            item.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
            item.Stock = request.Stock;
            item.IsAvailable = request.IsAvailable && (request.Stock is null || request.Stock > 0);
            item.UpdatedAt = now;
        }
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetItemAvailabilityAsync(int itemId, bool available, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        MenuItem item = await _repository.GetItemAsync(itemId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy món.");
        if (available && item.Stock == 0)
            throw new InvalidOperationException("Không thể mở bán món đã hết tồn kho.");
        item.IsAvailable = available;
        item.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ManagedChoiceGroupDto>> GetChoiceGroupsAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return (await _repository.GetChoiceGroupsAsync(cancellationToken))
            .Select(group => new ManagedChoiceGroupDto
            {
                ChoiceGroupId = group.ChoiceGroupId,
                GroupName = group.GroupName,
                IsAvailable = group.IsAvailable,
                IsRequired = group.IsRequired,
                MaxSelectDefault = group.MaxSelectDefault,
                Items = group.ChoiceItems.OrderBy(item => item.ChoiceName).Select(item => new ManagedChoiceItemDto
                {
                    ChoiceItemId = item.ChoiceItemId,
                    ChoiceName = item.ChoiceName,
                    ExtraPrice = item.ExtraPrice,
                    IsAvailable = item.IsAvailable,
                    ChannelPrices = item.ChannelPrices.Select(MapChannelPrice).ToList()
                }).ToList()
            }).ToList();
    }

    public async Task SaveChoiceGroupAsync(
        SaveChoiceGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        string name = request.GroupName.Trim();
        if (name.Length is < 1 or > 120)
            throw new InvalidOperationException("Tên nhóm lựa chọn phải từ 1 đến 120 ký tự.");
        if (request.MaxSelectDefault < 1)
            throw new InvalidOperationException("Số lựa chọn tối đa phải ít nhất là 1.");
        if (request.IsRequired && request.MaxSelectDefault != 1)
            throw new InvalidOperationException("Nhóm bắt buộc hiện hỗ trợ chọn đúng 1 lựa chọn.");
        if (await _repository.ChoiceGroupNameExistsAsync(name, request.ChoiceGroupId, cancellationToken))
            throw new InvalidOperationException("Tên nhóm lựa chọn đã tồn tại.");

        DateTime now = DateTime.UtcNow;
        if (request.ChoiceGroupId is null)
        {
            await _repository.AddChoiceGroupAsync(new ChoiceGroup
            {
                GroupName = name,
                IsAvailable = true,
                IsRequired = request.IsRequired,
                MaxSelectDefault = request.MaxSelectDefault,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }
        else
        {
            ChoiceGroup group = await _repository.GetChoiceGroupAsync(request.ChoiceGroupId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy nhóm lựa chọn.");
            group.GroupName = name;
            group.IsRequired = request.IsRequired;
            group.MaxSelectDefault = request.MaxSelectDefault;
            group.UpdatedAt = now;
        }
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChoiceItemAsync(
        SaveChoiceItemRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        ChoiceGroup group = await _repository.GetChoiceGroupAsync(request.ChoiceGroupId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy nhóm lựa chọn.");
        string name = request.ChoiceName.Trim();
        if (name.Length is < 1 or > 120)
            throw new InvalidOperationException("Tên lựa chọn phải từ 1 đến 120 ký tự.");
        if (request.ExtraPrice < 0)
            throw new InvalidOperationException("Giá cộng thêm không được âm.");
        if (await _repository.ChoiceItemNameExistsAsync(group.ChoiceGroupId, name, request.ChoiceItemId, cancellationToken))
            throw new InvalidOperationException("Tên lựa chọn đã tồn tại trong nhóm.");

        DateTime now = DateTime.UtcNow;
        if (request.ChoiceItemId is null)
        {
            await _repository.AddChoiceItemAsync(new ChoiceItem
            {
                ChoiceGroupId = group.ChoiceGroupId,
                ChoiceName = name,
                ExtraPrice = request.ExtraPrice,
                IsAvailable = true,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }
        else
        {
            ChoiceItem item = await _repository.GetChoiceItemAsync(request.ChoiceItemId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy lựa chọn.");
            if (item.ChoiceGroupId != group.ChoiceGroupId)
                throw new InvalidOperationException("Lựa chọn không thuộc nhóm đã chọn.");
            item.ChoiceName = name;
            item.ExtraPrice = request.ExtraPrice;
            item.UpdatedAt = now;
        }
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetChoiceGroupAvailabilityAsync(
        int choiceGroupId, bool available, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        ChoiceGroup group = await _repository.GetChoiceGroupAsync(choiceGroupId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy nhóm lựa chọn.");
        group.IsAvailable = available;
        group.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetChoiceItemAvailabilityAsync(
        int choiceItemId, bool available, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        ChoiceItem item = await _repository.GetChoiceItemAsync(choiceItemId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy lựa chọn.");
        item.IsAvailable = available;
        item.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignChoiceGroupAsync(
        AssignChoiceGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        _ = await _repository.GetItemAsync(request.MenuItemId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy món.");
        ChoiceGroup group = await _repository.GetChoiceGroupAsync(request.ChoiceGroupId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy nhóm lựa chọn.");
        int maxSelect = request.MaxSelect ?? group.MaxSelectDefault;
        if (maxSelect < 1 || (group.IsRequired && maxSelect != 1))
            throw new InvalidOperationException("Giới hạn lựa chọn không hợp lệ.");

        DateTime now = DateTime.UtcNow;
        MenuItemChoiceGroup? assignment = await _repository.GetAssignmentAsync(
            request.MenuItemId, request.ChoiceGroupId, cancellationToken);
        if (assignment is null)
        {
            await _repository.AddAssignmentAsync(new MenuItemChoiceGroup
            {
                MenuItemId = request.MenuItemId,
                ChoiceGroupId = request.ChoiceGroupId,
                DisplayOrder = request.DisplayOrder,
                MaxSelect = request.MaxSelect,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }
        else
        {
            assignment.DisplayOrder = request.DisplayOrder;
            assignment.MaxSelect = request.MaxSelect;
            assignment.UpdatedAt = now;
        }
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveChoiceGroupAssignmentAsync(
        int menuItemId, int choiceGroupId, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        MenuItemChoiceGroup assignment = await _repository.GetAssignmentAsync(menuItemId, choiceGroupId, cancellationToken)
            ?? throw new InvalidOperationException("Món chưa được gán nhóm lựa chọn này.");
        _repository.RemoveAssignment(assignment);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ManagedSalesChannelDto>> GetSalesChannelsAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        return (await _repository.GetSalesChannelsAsync(cancellationToken)).Select(channel => new ManagedSalesChannelDto
        {
            SalesChannelId = channel.SalesChannelId,
            ChannelCode = channel.ChannelCode,
            ChannelName = channel.ChannelName,
            IsActive = channel.IsActive
        }).ToList();
    }

    public async Task SaveSalesChannelAsync(
        SaveSalesChannelRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        string code = NormalizeChannelCode(request.ChannelCode);
        string name = request.ChannelName.Trim();
        if (name.Length is < 1 or > 120)
            throw new InvalidOperationException("Tên kênh bán phải từ 1 đến 120 ký tự.");
        if (await _repository.SalesChannelCodeExistsAsync(code, request.SalesChannelId, cancellationToken))
            throw new InvalidOperationException("Mã kênh bán đã tồn tại.");

        DateTime now = DateTime.UtcNow;
        if (request.SalesChannelId is null)
        {
            await _repository.AddSalesChannelAsync(new SalesChannel
            {
                ChannelCode = code,
                ChannelName = name,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }
        else
        {
            SalesChannel channel = await _repository.GetSalesChannelAsync(request.SalesChannelId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy kênh bán.");
            channel.ChannelCode = code;
            channel.ChannelName = name;
            channel.UpdatedAt = now;
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetSalesChannelActiveAsync(
        int salesChannelId,
        bool active,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        SalesChannel channel = await _repository.GetSalesChannelAsync(salesChannelId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy kênh bán.");
        channel.IsActive = active;
        channel.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveMenuItemChannelPriceAsync(
        SaveChannelPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        _ = await _repository.GetItemAsync(request.MenuItemId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy món.");
        if (request.ChannelExtraPrice < 0)
            throw new InvalidOperationException("Giá cộng thêm theo kênh không được âm.");
        if (!(await _repository.GetSalesChannelsAsync(cancellationToken))
            .Any(channel => channel.SalesChannelId == request.SalesChannelId && channel.IsActive))
            throw new InvalidOperationException("Kênh bán không tồn tại hoặc đang bị khóa.");

        MenuItemChannelPrice? price = await _repository.GetMenuItemChannelPriceAsync(
            request.MenuItemId, request.SalesChannelId, cancellationToken);
        if (price is null)
        {
            await _repository.AddMenuItemChannelPriceAsync(new MenuItemChannelPrice
            {
                MenuItemId = request.MenuItemId,
                SalesChannelId = request.SalesChannelId,
                ChannelExtraPrice = request.ChannelExtraPrice
            }, cancellationToken);
        }
        else
        {
            price.ChannelExtraPrice = request.ChannelExtraPrice;
        }
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChoiceItemChannelPriceAsync(
        SaveChannelPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        int choiceItemId = request.ChoiceItemId
            ?? throw new InvalidOperationException("Vui lòng chọn lựa chọn phụ.");
        _ = await _repository.GetChoiceItemAsync(choiceItemId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy lựa chọn phụ.");
        if (request.ChannelExtraPrice < 0)
            throw new InvalidOperationException("Giá cộng thêm theo kênh không được âm.");
        if (!(await _repository.GetSalesChannelsAsync(cancellationToken))
            .Any(channel => channel.SalesChannelId == request.SalesChannelId && channel.IsActive))
            throw new InvalidOperationException("Kênh bán không tồn tại hoặc đang bị khóa.");

        ChoiceItemChannelPrice? price = await _repository.GetChoiceItemChannelPriceAsync(
            choiceItemId, request.SalesChannelId, cancellationToken);
        if (price is null)
        {
            await _repository.AddChoiceItemChannelPriceAsync(new ChoiceItemChannelPrice
            {
                ChoiceItemId = choiceItemId,
                SalesChannelId = request.SalesChannelId,
                ChannelExtraPrice = request.ChannelExtraPrice
            }, cancellationToken);
        }
        else
        {
            price.ChannelExtraPrice = request.ChannelExtraPrice;
        }
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static ManagedChannelPriceDto MapChannelPrice(MenuItemChannelPrice price) => new()
    {
        SalesChannelId = price.SalesChannelId,
        ChannelCode = price.SalesChannel?.ChannelCode ?? string.Empty,
        ChannelName = price.SalesChannel?.ChannelName ?? string.Empty,
        ChannelExtraPrice = price.ChannelExtraPrice
    };

    private static ManagedChannelPriceDto MapChannelPrice(ChoiceItemChannelPrice price) => new()
    {
        SalesChannelId = price.SalesChannelId,
        ChannelCode = price.SalesChannel?.ChannelCode ?? string.Empty,
        ChannelName = price.SalesChannel?.ChannelName ?? string.Empty,
        ChannelExtraPrice = price.ChannelExtraPrice
    };

    private static string NormalizeChannelCode(string code)
    {
        string normalized = code.Trim().ToUpperInvariant().Replace(' ', '_');
        if (normalized.Length is < 2 or > 50)
            throw new InvalidOperationException("Mã kênh bán phải từ 2 đến 50 ký tự.");
        if (!normalized.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
            throw new InvalidOperationException("Mã kênh bán chỉ gồm chữ, số hoặc dấu gạch dưới.");
        return normalized;
    }

    private void EnsureAdmin()
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.User!.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Chỉ Admin được quản lý thực đơn.");
    }
}
