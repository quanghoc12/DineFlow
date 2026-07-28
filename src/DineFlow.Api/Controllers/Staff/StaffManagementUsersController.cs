using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/management/users")]
public sealed class StaffManagementUsersController : StaffControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public StaffManagementUsersController(
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _currentUserService = currentUserService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserSummary>>> Get(CancellationToken cancellationToken)
    {
        UseHeaderUser(_currentUserService);
        return Ok(await _userService.GetUsersAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        UseHeaderUser(_currentUserService);
        await _userService.CreateAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{userId:int}")]
    public async Task<IActionResult> Update(
        int userId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        UseHeaderUser(_currentUserService);
        request.UserId = userId;
        await _userService.UpdateAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{userId:int}/active")]
    public async Task<IActionResult> SetActive(
        int userId,
        [FromBody] SetUserActiveRequest request,
        CancellationToken cancellationToken)
    {
        UseHeaderUser(_currentUserService);
        await _userService.SetActiveAsync(userId, request.IsActive, cancellationToken);
        return NoContent();
    }

    [HttpPost("{userId:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        int userId,
        [FromBody] ResetUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        UseHeaderUser(_currentUserService);
        await _userService.ResetPasswordAsync(userId, request.CurrentPassword, request.NewPassword, cancellationToken);
        return NoContent();
    }
}

public sealed class SetUserActiveRequest
{
    public bool IsActive { get; set; }
}

public sealed class ResetUserPasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
