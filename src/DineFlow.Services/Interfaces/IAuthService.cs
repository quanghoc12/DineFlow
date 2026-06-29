using DineFlow.BusinessObjects.Auth.DTOs;

namespace DineFlow.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);
    void Logout();
}
