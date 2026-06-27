using DineFlow.BusinessObjects.Auth.Entities;
using DineFlow.DataAccessObjects.Auth;
using DineFlow.Repositories.Interfaces;

namespace DineFlow.Repositories.Impl;

public class RoleRepository : IRoleRepository
{
    private readonly RoleDAO _roleDao;

    public RoleRepository(RoleDAO roleDao)
    {
        _roleDao = roleDao;
    }

    public async Task<Role?> GetByIdAsync(int roleId)
    {
        return await _roleDao.GetByIdAsync(roleId);
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _roleDao.GetByNameAsync(roleName);
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _roleDao.GetAllAsync();
    }
}
