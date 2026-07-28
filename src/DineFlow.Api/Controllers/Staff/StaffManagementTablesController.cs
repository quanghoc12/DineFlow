using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/management/tables")]
public sealed class StaffManagementTablesController : StaffControllerBase
{
    private readonly ITableManagementService _tableManagementService;

    public StaffManagementTablesController(ITableManagementService tableManagementService)
    {
        _tableManagementService = tableManagementService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ManagedTableDto>>> GetTables(CancellationToken cancellationToken)
    {
        return Ok(await _tableManagementService.GetAllAsync(cancellationToken));
    }

    [HttpGet("areas")]
    public async Task<ActionResult<IReadOnlyList<ManagedAreaDto>>> GetAreas(CancellationToken cancellationToken)
    {
        return Ok(await _tableManagementService.GetAreasAsync(cancellationToken));
    }

    [HttpPost("areas")]
    public async Task<ActionResult<ManagedAreaDto>> SaveArea(
        [FromBody] SaveAreaRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _tableManagementService.SaveAreaAsync(request, cancellationToken));
    }

    [HttpPatch("areas/{areaId:int}/active")]
    public async Task<IActionResult> SetAreaActive(
        int areaId,
        [FromBody] SetAreaActiveRequest request,
        CancellationToken cancellationToken)
    {
        await _tableManagementService.SetAreaActiveAsync(areaId, request.IsActive, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<ManagedTableDto>> Create(
        [FromBody] CreateManagedTableRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _tableManagementService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{tableId:int}")]
    public async Task<IActionResult> Update(
        int tableId,
        [FromBody] UpdateManagedTableRequest request,
        CancellationToken cancellationToken)
    {
        request.TableId = tableId;
        await _tableManagementService.UpdateAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{tableId:int}/active")]
    public async Task<IActionResult> SetActive(
        int tableId,
        [FromBody] SetTableActiveRequest request,
        CancellationToken cancellationToken)
    {
        await _tableManagementService.SetActiveAsync(tableId, request.IsActive, cancellationToken);
        return NoContent();
    }

    [HttpPost("{tableId:int}/reset-qr")]
    public async Task<ActionResult<ManagedTableDto>> ResetQr(int tableId, CancellationToken cancellationToken)
    {
        return Ok(await _tableManagementService.ResetQrAsync(tableId, cancellationToken));
    }

    [HttpPost("{tableId:int}/reset-otp")]
    public async Task<ActionResult<ManagedTableDto>> ResetOtp(int tableId, CancellationToken cancellationToken)
    {
        return Ok(await _tableManagementService.ResetOtpAsync(tableId, cancellationToken));
    }
}

public sealed class SetAreaActiveRequest
{
    public bool IsActive { get; set; }
}

public sealed class SetTableActiveRequest
{
    public bool IsActive { get; set; }
}
