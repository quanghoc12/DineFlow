using DineFlow.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/sessions")]
public class StaffSessionsController : StaffControllerBase
{
    private readonly ITableSessionService _tableSessionService;

    public StaffSessionsController(ITableSessionService tableSessionService)
    {
        _tableSessionService = tableSessionService;
    }

    [HttpGet("active")]
    public async Task<ActionResult<IReadOnlyList<TableSessionDto>>> GetActive(CancellationToken cancellationToken)
    {
        IReadOnlyList<TableSessionDto> response = await _tableSessionService.GetActiveSessionsAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TableSessionDetailDto>> GetDetail(int id, CancellationToken cancellationToken)
    {
        TableSessionDetailDto? response = await _tableSessionService.GetSessionDetailAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("{id:int}/close-if-completed")]
    public async Task<ActionResult<object>> CloseIfCompleted(int id, CancellationToken cancellationToken)
    {
        bool closed = await _tableSessionService.CloseSessionIfCompletedAsync(id, CurrentUserId, cancellationToken);
        return Ok(new { closed });
    }

    [HttpPost("{id:int}/move-table")]
    public async Task<ActionResult<TableSessionDto>> MoveTable(
        int id,
        [FromBody] MoveTableSessionRequest request,
        CancellationToken cancellationToken)
    {
        TableSessionDto response = await _tableSessionService.MoveTableAsync(id, request, cancellationToken);
        return Ok(response);
    }
}
