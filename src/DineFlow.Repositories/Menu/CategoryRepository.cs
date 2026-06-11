using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public class CategoryRepository : ICategoryRepository
{
    private readonly CategoryDAO _categoryDAO = new();

    public List<Category> GetAll() => _categoryDAO.GetAll();
    public Category? GetById(int id) => _categoryDAO.GetById(id);
    public Category Add(Category category) => _categoryDAO.Add(category);
    public void Update(Category category) => _categoryDAO.Update(category);
}
