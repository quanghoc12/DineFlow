using DineFlow.BusinessObjects.Auth;
using DineFlow.Repositories.Auth;

namespace DineFlow.Services.Auth;

public sealed class UserService : IUserService
{
    private static readonly HashSet<string> AllowedRoles =
        new(StringComparer.OrdinalIgnoreCase) { "Admin", "Staff" };

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        List<User> users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(Map).ToList();
    }

    public async Task CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        Validate(request.Username, request.FullName, request.Role);
        ValidatePassword(request.Password);

        if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");
        }

        DateTime now = DateTime.UtcNow;
        await _userRepository.AddAsync(new User
        {
            Username = request.Username.Trim(),
            FullName = request.FullName.Trim(),
            Role = NormalizeRole(request.Role),
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        }, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        Validate(request.Username, request.FullName, request.Role);
        User user = await FindUserAsync(request.UserId, cancellationToken);

        if (await _userRepository.UsernameExistsAsync(
                request.Username,
                request.UserId,
                cancellationToken))
        {
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");
        }

        if (user.IsActive &&
            user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
            !request.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
            await _userRepository.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            throw new InvalidOperationException("Không thể đổi vai trò của Admin hoạt động cuối cùng.");
        }

        user.Username = request.Username.Trim();
        user.FullName = request.FullName.Trim();
        user.Role = NormalizeRole(request.Role);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetActiveAsync(
        int userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        User user = await FindUserAsync(userId, cancellationToken);

        if (!isActive && user.UserId == _currentUserService.User?.UserId)
        {
            throw new InvalidOperationException("Bạn không thể khóa tài khoản đang đăng nhập.");
        }

        if (!isActive &&
            user.IsActive &&
            user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
            await _userRepository.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            throw new InvalidOperationException("Không thể khóa Admin hoạt động cuối cùng.");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(
        int userId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        ValidatePassword(newPassword);
        User user = await FindUserAsync(userId, cancellationToken);
        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    private void EnsureAdmin()
    {
        if (!_currentUserService.IsAuthenticated ||
            !_currentUserService.User!.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Chỉ Admin được quản lý người dùng.");
        }
    }

    private async Task<User> FindUserAsync(int userId, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy người dùng.");
    }

    private static void Validate(string username, string fullName, string role)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Trim().Length < 3)
        {
            throw new InvalidOperationException("Tên đăng nhập phải có ít nhất 3 ký tự.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Họ tên không được để trống.");
        }

        if (!AllowedRoles.Contains(role))
        {
            throw new InvalidOperationException("Vai trò chỉ có thể là Admin hoặc Staff.");
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            throw new InvalidOperationException("Mật khẩu phải có ít nhất 6 ký tự.");
        }
    }

    private static string NormalizeRole(string role) =>
        role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Staff";

    private static UserSummary Map(User user) => new()
    {
        UserId = user.UserId,
        Username = user.Username,
        FullName = user.FullName,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
