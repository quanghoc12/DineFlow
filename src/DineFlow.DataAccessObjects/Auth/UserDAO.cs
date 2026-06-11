using DineFlow.BusinessObjects.Auth;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Auth;

public class UserDAO
{
    public List<User> GetAll()
    {
        using var db = new AppDbContext();
        return db.Users.AsNoTracking().OrderBy(x => x.UserId).ToList();
    }

    public User? GetById(int id)
    {
        using var db = new AppDbContext();
        return db.Users.AsNoTracking().FirstOrDefault(x => x.UserId == id);
    }

    public User? GetByUsername(string username)
    {
        using var db = new AppDbContext();
        return db.Users.AsNoTracking().FirstOrDefault(x => x.Username == username);
    }

    public User Add(User user)
    {
        using var db = new AppDbContext();
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public void Update(User user)
    {
        using var db = new AppDbContext();
        db.Users.Update(user);
        db.SaveChanges();
    }
}
