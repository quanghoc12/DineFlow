using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.Menu;

namespace DineFlow.Repositories.Menu;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly MenuItemDAO _menuItemDAO = new();

    public List<MenuItem> GetAll() => _menuItemDAO.GetAll();
    public MenuItem? GetById(int id) => _menuItemDAO.GetById(id);
    public List<MenuItem> Search(string keyword) => _menuItemDAO.Search(keyword);
    public MenuItem Add(MenuItem item) => _menuItemDAO.Add(item);
    public void Update(MenuItem item) => _menuItemDAO.Update(item);
}
