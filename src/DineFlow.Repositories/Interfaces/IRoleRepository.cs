using DineFlow.BusinessObjects.Auth.Entities;

namespace DineFlow.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int roleId);
    Task<Role?> GetByNameAsync(string roleName);
    Task<List<Role>> GetAllAsync();
}
