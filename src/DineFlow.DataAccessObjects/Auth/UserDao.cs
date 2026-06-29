using DineFlow.BusinessObjects.Auth;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Auth;

public sealed class UserDao : IUserDao
{
    private readonly AppDbContext _dbContext;

    public UserDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Username)
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        string normalizedUsername = username.Trim().ToLower();
        return _dbContext.Users.FirstOrDefaultAsync(
            user => user.Username.ToLower() == normalizedUsername,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(
        string username,
        int? excludedUserId = null,
        CancellationToken cancellationToken = default)
    {
        string normalizedUsername = username.Trim().ToLower();
        return _dbContext.Users.AnyAsync(
            user => user.Username.ToLower() == normalizedUsername &&
                    (!excludedUserId.HasValue || user.UserId != excludedUserId.Value),
            cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.CountAsync(
            user => user.IsActive && user.Role.ToLower() == "admin",
            cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }
}
