using DineFlow.BusinessObjects.Auth;

namespace DineFlow.WPFApp.Services.Authorization;

public static class ApiClientSession
{
    public static int CurrentUserId { get; private set; } = 1;
    public static string CurrentUserRole { get; private set; } = "Staff";

    public static void Configure(CurrentUser? user)
    {
        if (user is null)
        {
            Clear();
            return;
        }

        CurrentUserId = user.UserId;
        CurrentUserRole = string.IsNullOrWhiteSpace(user.Role) ? "Staff" : user.Role.Trim();
    }

    public static void Clear()
    {
        CurrentUserId = 1;
        CurrentUserRole = "Staff";
    }
}
