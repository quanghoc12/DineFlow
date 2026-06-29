using DineFlow.BusinessObjects.Auth.Constants;
using DineFlow.BusinessObjects.Auth.DTOs;
using DineFlow.BusinessObjects.Auth.Entities;
using DineFlow.Repositories.Interfaces;
using DineFlow.Services.Interfaces;

namespace DineFlow.Services.Impl;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<List<UserDisplayDto>> GetUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDisplayDto
        {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName,
            RoleName = u.Role?.RoleName ?? string.Empty,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<UserDisplayDto?> GetUserByIdAsync(int userId)
    {
        var u = await _userRepository.GetByIdAsync(userId);
        if (u == null) return null;
        
        return new UserDisplayDto
        {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName,
            RoleName = u.Role?.RoleName ?? string.Empty,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }

    public async Task CreateUserAsync(CreateUserRequestDto request)
    {
        var exists = await _userRepository.ExistsByUsernameAsync(request.Username);
        if (exists)
            throw new InvalidOperationException("Username already exists.");

        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role == null)
            throw new InvalidOperationException("Role does not exist.");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName,
            RoleId = request.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Note: The User entity design (SRS 7.1) does not include CreatedBy/UpdatedBy.
        // Therefore ICurrentUserService.GetCurrentUserId() is injected but currently not assigned 
        // to any audit fields for the User table. It is kept available if schema changes.

        await _userRepository.CreateAsync(user);
    }

    public async Task UpdateUserAsync(UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (user.Username != request.Username)
        {
            var exists = await _userRepository.ExistsByUsernameAsync(request.Username);
            if (exists)
                throw new InvalidOperationException("Username already exists.");
        }

        var role = await _roleRepository.GetByIdAsync(request.RoleId);
        if (role == null)
            throw new InvalidOperationException("Role does not exist.");

        user.Username = request.Username;
        user.FullName = request.FullName;
        user.RoleId = request.RoleId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task DisableUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (user.Role?.RoleName == Roles.Admin && user.IsActive)
        {
            var allUsers = await _userRepository.GetAllAsync();
            var activeAdminsCount = allUsers.Count(u => u.IsActive && u.Role?.RoleName == Roles.Admin);
            
            if (activeAdminsCount <= 1)
            {
                throw new InvalidOperationException("Cannot disable the last active Admin.");
            }
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task EnableUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }
}
