using DineFlow.BusinessObjects.Auth;

namespace DineFlow.Services.Auth;

public interface IAuthService
{
    CurrentUserDto Login(LoginRequestDto request);
}
