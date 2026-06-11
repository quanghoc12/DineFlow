using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public interface ICategoryRepository
{
    List<Category> GetAll();
    Category? GetById(int id);
    Category Add(Category category);
    void Update(Category category);
}
