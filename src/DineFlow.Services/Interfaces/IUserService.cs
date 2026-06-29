using DineFlow.BusinessObjects.Auth.DTOs;
using DineFlow.BusinessObjects.Auth.Entities;

namespace DineFlow.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDisplayDto>> GetUsersAsync();
    Task<UserDisplayDto?> GetUserByIdAsync(int userId);
    Task CreateUserAsync(CreateUserRequestDto request);
    Task UpdateUserAsync(UpdateUserRequestDto request);
    Task DisableUserAsync(int userId);
    Task EnableUserAsync(int userId);
    Task ResetPasswordAsync(ResetPasswordRequestDto request);
}
