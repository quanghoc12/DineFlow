using DineFlow.BusinessObjects.Menu;

namespace DineFlow.Services.Menu;

public interface IMenuItemService
{
    List<MenuItem> GetAll();
    List<MenuItem> Search(string keyword);
    MenuItem Create(MenuItem item);
    void Update(MenuItem item);
}
