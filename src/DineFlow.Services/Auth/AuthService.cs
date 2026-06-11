using DineFlow.BusinessObjects.Auth;
using DineFlow.Repositories.Auth;

namespace DineFlow.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService() : this(new UserRepository())
    {
    }

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public CurrentUserDto Login(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new Exception("Username không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new Exception("Password không được để trống.");
        }

        var user = _userRepository.GetByUsername(request.Username.Trim());

        if (user == null || !user.IsActive)
        {
            throw new Exception("Tài khoản không tồn tại hoặc đã bị khóa.");
        }

        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            throw new Exception("Username hoặc password không đúng.");
        }

        return new CurrentUserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}
