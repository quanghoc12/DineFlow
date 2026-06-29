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
        _dbContext.Categories.AsNoTracking()
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
            .Include(item => item.Category)
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
                    (!excludedId.HasValue || item.MenuItemId != excludedId.Value),
            cancellationToken);
    }

    public Task AddItemAsync(MenuItem item, CancellationToken cancellationToken = default) =>
        _dbContext.MenuItems.AddAsync(item, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
