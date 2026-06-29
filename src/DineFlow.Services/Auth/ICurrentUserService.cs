using DineFlow.BusinessObjects.Auth;

namespace DineFlow.Services.Auth;

public interface ICurrentUserService
{
    CurrentUser? User { get; }
    bool IsAuthenticated { get; }
    void Login(CurrentUser user);
    void Logout();
}
