using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

public abstract class StaffControllerBase : ControllerBase
{
    protected void UseHeaderUser(ICurrentUserService currentUserService)
    {
        currentUserService.Login(new CurrentUser
        {
            UserId = CurrentUserId,
            Username = Request.Headers.TryGetValue("X-User-Name", out var usernameValues)
                ? usernameValues.FirstOrDefault() ?? $"staff-{CurrentUserId}"
                : $"staff-{CurrentUserId}",
            FullName = Request.Headers.TryGetValue("X-User-FullName", out var fullNameValues)
                ? fullNameValues.FirstOrDefault() ?? AuthRoles.GetLabel(CurrentUserRole)
                : AuthRoles.GetLabel(CurrentUserRole),
            Role = AuthRoles.Normalize(CurrentUserRole)
        });
    }

    protected int CurrentUserId
    {
        get
        {
            if (Request.Headers.TryGetValue("X-User-Id", out var values) &&
                int.TryParse(values.FirstOrDefault(), out int userId))
            {
                return userId;
            }

            return 1;
        }
    }

    protected string CurrentUserRole
    {
        get
        {
            if (Request.Headers.TryGetValue("X-User-Role", out var values))
            {
                string? role = values.FirstOrDefault()?.Trim();
                if (!string.IsNullOrWhiteSpace(role))
                {
                    return role;
                }
            }

            return "Staff";
        }
    }
}
