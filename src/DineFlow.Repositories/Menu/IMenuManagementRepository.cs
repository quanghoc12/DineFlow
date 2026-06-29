using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public interface IMenuManagementRepository
{
    Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<bool> CategoryNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken = default);
    Task<MenuItem?> GetItemAsync(int itemId, CancellationToken cancellationToken = default);
    Task<bool> ItemNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddItemAsync(MenuItem item, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
