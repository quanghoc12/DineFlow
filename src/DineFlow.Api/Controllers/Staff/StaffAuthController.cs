using DineFlow.Api.Services;
using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/auth")]
public sealed class StaffAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IStaffAuthTokenService _tokenService;

    public StaffAuthController(IAuthService authService, IStaffAuthTokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<StaffLoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        LoginResult result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.IsSuccess || result.User is null)
        {
            return Unauthorized(new { message = result.ErrorMessage ?? "Login failed." });
        }

        string token = _tokenService.CreateToken(result.User);
        return Ok(new StaffLoginResponse
        {
            Token = token,
            TokenType = "Bearer",
            UserId = result.User.UserId,
            Username = result.User.Username,
            FullName = result.User.FullName,
            Role = AuthRoles.Normalize(result.User.Role)
        });
    }
}

public sealed class StaffLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
