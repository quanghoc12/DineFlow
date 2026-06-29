using DineFlow.BusinessObjects.Auth;
using DineFlow.Repositories.Auth;

namespace DineFlow.Services.Auth;

public sealed class UserService : IUserService
{
    private static readonly HashSet<string> AllowedRoles =
        new(StringComparer.OrdinalIgnoreCase) { AuthRoles.Owner, AuthRoles.Admin, AuthRoles.Staff };

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
        if (!IsOwner() && AuthRoles.IsOwner(request.Role))
            throw new UnauthorizedAccessException("Chỉ Chủ nhà hàng được tạo tài khoản Chủ nhà hàng.");

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
        EnsureRoleChangeAllowed(user, request.Role);

        if (await _userRepository.UsernameExistsAsync(
                request.Username,
                request.UserId,
                cancellationToken))
        {
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");
        }

        user.Username = request.Username.Trim();
        user.FullName = request.FullName.Trim();
        user.Role = NormalizeRole(request.Role);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);
        RefreshCurrentSession(user);
    }

    public async Task SetActiveAsync(
        int userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        User user = await FindUserAsync(userId, cancellationToken);

        if (!IsOwner() && (AuthRoles.IsAdmin(user.Role) || AuthRoles.IsOwner(user.Role)))
            throw new UnauthorizedAccessException("Admin không được khóa tài khoản Admin hoặc Chủ nhà hàng.");

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);
        if (!isActive && user.UserId == _currentUserService.User?.UserId)
            _currentUserService.Logout();
    }

    public async Task ResetPasswordAsync(
        int userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin();
        ValidatePassword(newPassword);
        User user = await FindUserAsync(userId, cancellationToken);
        if (!IsOwner() && !_passwordHasher.Verify(currentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Mật khẩu cũ không chính xác.");
        }
        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    private void EnsureAdmin()
    {
        if (!_currentUserService.IsAuthenticated ||
            !AuthRoles.CanManage(_currentUserService.User!.Role))
        {
            throw new UnauthorizedAccessException("Chỉ Admin hoặc Chủ nhà hàng được quản lý người dùng.");
        }
    }

    private bool IsOwner() =>
        AuthRoles.IsOwner(_currentUserService.User?.Role);

    private void EnsureRoleChangeAllowed(User target, string requestedRole)
    {
        if (IsOwner()) return;

        if (AuthRoles.IsOwner(target.Role))
            throw new UnauthorizedAccessException("Admin không được thay đổi vai trò Chủ nhà hàng.");

        bool sameRole = target.Role.Equals(requestedRole, StringComparison.OrdinalIgnoreCase);
        bool promotesStaff = AuthRoles.IsStaff(target.Role) && AuthRoles.IsAdmin(requestedRole);
        if (!sameRole && !promotesStaff)
            throw new UnauthorizedAccessException("Admin chỉ được phép chuyển Staff lên Admin.");
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
            throw new InvalidOperationException("Vai trò chỉ có thể là Chủ nhà hàng, Admin hoặc Staff.");
        }
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            throw new InvalidOperationException("Mật khẩu phải có ít nhất 6 ký tự.");
        }
    }

    private static string NormalizeRole(string role) => AuthRoles.Normalize(role);

    private void RefreshCurrentSession(User user)
    {
        if (user.UserId != _currentUserService.User?.UserId) return;
        _currentUserService.Login(new CurrentUser
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role
        });
    }

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
