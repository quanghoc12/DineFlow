using DineFlow.BusinessObjects.Auth.Entities;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Auth;

public class RoleDAO
{
    private readonly AppDbContext _context;

    public RoleDAO(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(int roleId)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
    }

    public async Task<List<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }
}
