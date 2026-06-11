using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Menu;

public class MenuItemDAO
{
    public List<MenuItem> GetAll()
    {
        using var db = new AppDbContext();
        return db.MenuItems.Include(x => x.Category).AsNoTracking().OrderBy(x => x.MenuItemId).ToList();
    }

    public MenuItem? GetById(int id)
    {
        using var db = new AppDbContext();
        return db.MenuItems.Include(x => x.Category).FirstOrDefault(x => x.MenuItemId == id);
    }

    public List<MenuItem> Search(string keyword)
    {
        using var db = new AppDbContext();
        return db.MenuItems
            .Include(x => x.Category)
            .AsNoTracking()
            .Where(x => x.ItemName.Contains(keyword))
            .OrderBy(x => x.ItemName)
            .ToList();
    }

    public MenuItem Add(MenuItem item)
    {
        using var db = new AppDbContext();
        db.MenuItems.Add(item);
        db.SaveChanges();
        return item;
    }

    public void Update(MenuItem item)
    {
        using var db = new AppDbContext();
        db.MenuItems.Update(item);
        db.SaveChanges();
    }
}
