using DineFlow.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/tables")]
public class StaffTablesController : StaffControllerBase
{
    private readonly ITableSessionService _tableSessionService;

    public StaffTablesController(ITableSessionService tableSessionService)
    {
        _tableSessionService = tableSessionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DiningTableDto>>> GetTables(
        [FromQuery] string? status,
        [FromQuery] string? area,
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<DiningTableDto> response = await _tableSessionService.GetTablesAsync(new DiningTableFilter
        {
            Status = status,
            Area = area,
            ActiveOnly = activeOnly ?? true
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{tableId:int}/current-session")]
    public async Task<ActionResult<TableSessionDto>> GetCurrentSession(
        int tableId,
        CancellationToken cancellationToken)
    {
        TableSessionDto? response = await _tableSessionService.GetCurrentSessionByTableIdAsync(tableId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("{tableId:int}/session")]
    public async Task<ActionResult<TableSessionDto>> GetOrCreateSession(
        int tableId,
        CancellationToken cancellationToken)
    {
        TableSessionDto response = await _tableSessionService.GetOrCreateActiveSessionByTableIdAsync(
            tableId,
            CurrentUserId,
            cancellationToken);

        return Ok(response);
    }
}
