using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public interface IMenuItemRepository
{
    List<MenuItem> GetAll();
    MenuItem? GetById(int id);
    List<MenuItem> Search(string keyword);
    MenuItem Add(MenuItem item);
    void Update(MenuItem item);
}
