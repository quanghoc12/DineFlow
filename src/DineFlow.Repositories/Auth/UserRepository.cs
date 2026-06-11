using DineFlow.BusinessObjects.Auth;
using DineFlow.DataAccessObjects.Auth;

namespace DineFlow.Repositories.Auth;

public class UserRepository : IUserRepository
{
    private readonly UserDAO _userDAO = new();

    public List<User> GetAll() => _userDAO.GetAll();
    public User? GetById(int id) => _userDAO.GetById(id);
    public User? GetByUsername(string username) => _userDAO.GetByUsername(username);
    public User Add(User user) => _userDAO.Add(user);
    public void Update(User user) => _userDAO.Update(user);
}
