using DineFlow.Api.Services;
using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/table-otps")]
public sealed class StaffTableOtpsController : ControllerBase
{
    private readonly IStaffAuthTokenService _tokenService;
    private readonly ITableOtpService _tableOtpService;

    public StaffTableOtpsController(
        IStaffAuthTokenService tokenService,
        ITableOtpService tableOtpService)
    {
        _tokenService = tokenService;
        _tableOtpService = tableOtpService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffTableOtpDto>>> Get(
        [FromQuery] int? areaId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        StaffAuthPrincipal? principal = ResolvePrincipal();
        if (principal is null)
        {
            return Unauthorized();
        }

        IReadOnlyList<StaffTableOtpDto> response = await _tableOtpService.GetAsync(
            new TableOtpFilter
            {
                AreaId = areaId,
                Status = status,
                Search = search
            },
            principal.Role,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("{tableId:int}/reset")]
    public async Task<ActionResult<StaffTableOtpDto>> Reset(
        int tableId,
        CancellationToken cancellationToken)
    {
        StaffAuthPrincipal? principal = ResolvePrincipal();
        if (principal is null)
        {
            return Unauthorized();
        }
        if (!AuthRoles.IsAdmin(principal.Role))
        {
            return Forbid();
        }

        StaffTableOtpDto response = await _tableOtpService.ResetAsync(tableId, principal.Role, cancellationToken);
        return Ok(response);
    }

    [HttpPost("reset")]
    public async Task<ActionResult<IReadOnlyList<StaffTableOtpDto>>> ResetBatch(
        [FromBody] ResetTableOtpBatchRequest request,
        CancellationToken cancellationToken)
    {
        StaffAuthPrincipal? principal = ResolvePrincipal();
        if (principal is null)
        {
            return Unauthorized();
        }
        if (!AuthRoles.IsAdmin(principal.Role))
        {
            return Forbid();
        }

        IReadOnlyList<StaffTableOtpDto> response = await _tableOtpService.ResetBatchAsync(
            request,
            principal.Role,
            cancellationToken);
        return Ok(response);
    }

    private StaffAuthPrincipal? ResolvePrincipal()
    {
        string? header = Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(header) &&
            header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return _tokenService.ValidateToken(header["Bearer ".Length..].Trim());
        }

        if (Request.Headers.TryGetValue("X-User-Role", out var roleValues))
        {
            string role = roleValues.FirstOrDefault()?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(role))
            {
                int userId = 1;
                if (Request.Headers.TryGetValue("X-User-Id", out var userIdValues) &&
                    int.TryParse(userIdValues.FirstOrDefault(), out int parsedUserId))
                {
                    userId = parsedUserId;
                }

                return new StaffAuthPrincipal(
                    userId,
                    string.Empty,
                    string.Empty,
                    AuthRoles.Normalize(role),
                    DateTime.UtcNow.AddMinutes(5));
            }
        }

        return null;
    }
}
