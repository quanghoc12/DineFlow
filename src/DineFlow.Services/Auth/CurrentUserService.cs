using DineFlow.BusinessObjects.Auth;

namespace DineFlow.Services.Auth;

public sealed class CurrentUserService : ICurrentUserService
{
    public CurrentUser? User { get; private set; }
    public bool IsAuthenticated => User is not null;

    public void Login(CurrentUser user)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
    }

    public void Logout()
    {
        User = null;
    }
}
