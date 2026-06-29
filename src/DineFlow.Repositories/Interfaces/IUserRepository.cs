using DineFlow.BusinessObjects.Auth.Entities;

namespace DineFlow.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByUsernameAsync(string username);
    Task<List<User>> GetAllAsync();
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> ExistsByUsernameAsync(string username);
}
