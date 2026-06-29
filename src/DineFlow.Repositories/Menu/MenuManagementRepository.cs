using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public sealed class MenuManagementRepository : IMenuManagementRepository
{
    private readonly IMenuManagementDao _dao;

    public MenuManagementRepository(IMenuManagementDao dao) => _dao = dao;

    public Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default) => _dao.GetCategoriesAsync(cancellationToken);
    public Task<Category?> GetCategoryAsync(int categoryId, CancellationToken cancellationToken = default) => _dao.GetCategoryAsync(categoryId, cancellationToken);
    public Task<bool> CategoryNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default) => _dao.CategoryNameExistsAsync(name, excludedId, cancellationToken);
    public Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default) => _dao.AddCategoryAsync(category, cancellationToken);
    public Task<List<MenuItem>> GetItemsAsync(CancellationToken cancellationToken = default) => _dao.GetItemsAsync(cancellationToken);
    public Task<MenuItem?> GetItemAsync(int itemId, CancellationToken cancellationToken = default) => _dao.GetItemAsync(itemId, cancellationToken);
    public Task<bool> ItemNameExistsAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default) => _dao.ItemNameExistsAsync(name, excludedId, cancellationToken);
    public Task AddItemAsync(MenuItem item, CancellationToken cancellationToken = default) => _dao.AddItemAsync(item, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _dao.SaveChangesAsync(cancellationToken);
}
