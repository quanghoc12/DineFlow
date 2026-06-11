using DineFlow.BusinessObjects.Menu;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Menu;

public class CategoryDAO
{
    public List<Category> GetAll()
    {
        using var db = new AppDbContext();
        return db.Categories.AsNoTracking().OrderBy(x => x.DisplayOrder).ToList();
    }

    public Category? GetById(int id)
    {
        using var db = new AppDbContext();
        return db.Categories.FirstOrDefault(x => x.CategoryId == id);
    }

    public Category Add(Category category)
    {
        using var db = new AppDbContext();
        db.Categories.Add(category);
        db.SaveChanges();
        return category;
    }

    public void Update(Category category)
    {
        using var db = new AppDbContext();
        db.Categories.Update(category);
        db.SaveChanges();
    }
}
