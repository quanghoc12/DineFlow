using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu;

public interface ICategoryService
{
    List<Category> GetAll();
    Category Create(Category category);
    void Update(Category category);
}
