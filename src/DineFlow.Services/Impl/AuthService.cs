using DineFlow.BusinessObjects.Auth.DTOs;
using DineFlow.Repositories.Interfaces;
using DineFlow.Services.Interfaces;

namespace DineFlow.Services.Impl;

public class AuthService : IAuthService
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

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        // 1. Find user by username
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        // 2. If user not found
        if (user == null)
        {
            return new LoginResultDto 
            { 
                IsSuccess = false, 
                ErrorMessage = "Invalid username or password." 
            };
        }

        // 3. Verify password using IPasswordHasher
        // 4. If password invalid
        bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return new LoginResultDto 
            { 
                IsSuccess = false, 
                ErrorMessage = "Invalid username or password." 
            };
        }

        // 5. Check IsActive
        // 6. If inactive
        if (!user.IsActive)
        {
            return new LoginResultDto 
            { 
                IsSuccess = false, 
                ErrorMessage = "Account has been disabled." 
            };
        }

        // 7. Load Role (already loaded by Repository Include)
        var roleName = user.Role?.RoleName ?? string.Empty;

        // 8. Create CurrentUserDto
        var currentUserDto = new CurrentUserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Role = roleName
        };

        // 9. Call CurrentUserService.Login()
        _currentUserService.Login(currentUserDto);

        // 10. Return LoginResultDto(Success)
        return new LoginResultDto
        {
            IsSuccess = true,
            User = currentUserDto
        };
    }

    public void Logout()
    {
        _currentUserService.Logout();
    }
}
