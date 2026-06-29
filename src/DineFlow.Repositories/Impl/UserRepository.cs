using DineFlow.BusinessObjects.Auth.Entities;
using DineFlow.DataAccessObjects.Auth;
using DineFlow.Repositories.Interfaces;

namespace DineFlow.Repositories.Impl;

public class UserRepository : IUserRepository
{
    private readonly UserDAO _userDao;

    public UserRepository(UserDAO userDao)
    {
        _userDao = userDao;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _userDao.GetByIdAsync(userId);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _userDao.GetByUsernameAsync(username);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _userDao.GetAllAsync();
    }

    public async Task CreateAsync(User user)
    {
        await _userDao.CreateAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _userDao.UpdateAsync(user);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _userDao.ExistsByUsernameAsync(username);
    }
}
