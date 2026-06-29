using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

public abstract class StaffControllerBase : ControllerBase
{
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
}
