using DineFlow.BusinessObjects.Auth;

namespace DineFlow.Services.Auth;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    void Logout();
}
