using DineFlow.BusinessObjects.Auth;
using DineFlow.Repositories.Auth;

namespace DineFlow.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        string username = request.Username.Trim();
        User? user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Failed("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        if (!user.IsActive)
        {
            return Failed("Tài khoản đã bị vô hiệu hóa.");
        }

        if (_passwordHasher.NeedsUpgrade(user.PasswordHash))
        {
            user.PasswordHash = _passwordHasher.Hash(request.Password);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        CurrentUser currentUser = new()
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role
        };

        _currentUserService.Login(currentUser);
        return new LoginResult { IsSuccess = true, User = currentUser };
    }

    public void Logout()
    {
        _currentUserService.Logout();
    }

    private static LoginResult Failed(string message)
    {
        return new LoginResult { ErrorMessage = message };
    }
}
