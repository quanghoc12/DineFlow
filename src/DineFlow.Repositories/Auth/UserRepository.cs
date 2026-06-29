using DineFlow.BusinessObjects.Auth;
using DineFlow.DataAccessObjects.Auth;

namespace DineFlow.Repositories.Auth;

public sealed class UserRepository : IUserRepository
{
    private readonly IUserDao _userDao;

    public UserRepository(IUserDao userDao)
    {
        _userDao = userDao;
    }

    public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _userDao.GetAllAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _userDao.GetByIdAsync(userId, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _userDao.GetByUsernameAsync(username, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _userDao.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> UsernameExistsAsync(
        string username,
        int? excludedUserId = null,
        CancellationToken cancellationToken = default)
    {
        return _userDao.UsernameExistsAsync(username, excludedUserId, cancellationToken);
    }

    public Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default)
    {
        return _userDao.CountActiveAdminsAsync(cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return _userDao.AddAsync(user, cancellationToken);
    }
}
