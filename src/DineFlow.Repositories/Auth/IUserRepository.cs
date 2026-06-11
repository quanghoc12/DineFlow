using DineFlow.BusinessObjects.Auth;

namespace DineFlow.Repositories.Auth;

public interface IUserRepository
{
    List<User> GetAll();
    User? GetById(int id);
    User? GetByUsername(string username);
    User Add(User user);
    void Update(User user);
}
