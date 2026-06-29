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
            Stock = item.Stock
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

    private void EnsureAdmin()
    {
        if (!_currentUser.IsAuthenticated ||
            !_currentUser.User!.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Chỉ Admin được quản lý thực đơn.");
    }
}
