using DineFlow.Services.Bills;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/bills")]
public class StaffBillsController : StaffControllerBase
{
    private readonly IBillService _billService;
    private readonly ISplitBillService _splitBillService;

    public StaffBillsController(IBillService billService, ISplitBillService splitBillService)
    {
        _billService = billService;
        _splitBillService = splitBillService;
    }

    [HttpGet("session/{tableSessionId:int}")]
    public async Task<ActionResult<IReadOnlyList<BillSummaryDto>>> GetBySession(
        int tableSessionId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BillSummaryDto> response = await _billService.GetBillsBySessionAsync(tableSessionId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{billId:int}")]
    public async Task<ActionResult<BillDto>> GetBill(int billId, CancellationToken cancellationToken)
    {
        BillDto? response = await _billService.GetBillByIdAsync(billId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("session/{tableSessionId:int}/default")]
    public async Task<ActionResult<BillDto>> GetOrCreateDefault(
        int tableSessionId,
        CancellationToken cancellationToken)
    {
        BillDto response = await _billService.GetOrCreateDefaultBillAsync(tableSessionId, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("details/{billDetailId:int}/quantity")]
    public async Task<ActionResult<BillDto>> AdjustBillDetailQuantity(
        int billDetailId,
        [FromBody] AdjustBillDetailQuantityRequest request,
        CancellationToken cancellationToken)
    {
        request.BillDetailId = billDetailId;
        BillDto response = await _billService.AdjustBillDetailQuantityAsync(request, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{billId:int}/notify")]
    public async Task<ActionResult<BillDto>> NotifyBill(int billId, CancellationToken cancellationToken)
    {
        BillDto response = await _billService.NotifyBillAsync(billId, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{billId:int}/name")]
    public async Task<ActionResult<BillDto>> RenameBill(
        int billId,
        [FromBody] RenameBillRequest request,
        CancellationToken cancellationToken)
    {
        BillDto response = await _billService.RenameBillAsync(billId, request.BillName, cancellationToken);
        return Ok(response);
    }

    [HttpPost("split")]
    public async Task<ActionResult<BillDto>> Split(
        [FromBody] SplitBillRequest request,
        CancellationToken cancellationToken)
    {
        BillDto response = await _splitBillService.CreateSplitBillAsync(request, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("split-batch")]
    public async Task<ActionResult<BillDto>> SplitBatch(
        [FromBody] SplitBillBatchRequest request,
        CancellationToken cancellationToken)
    {
        BillDto response = await _splitBillService.SplitBillBatchAsync(request, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("merge")]
    public async Task<ActionResult<BillDto>> Merge(
        [FromBody] MergeBillRequest request,
        CancellationToken cancellationToken)
    {
        BillDto response = await _splitBillService.MergeBillAsync(request, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("move-item")]
    public async Task<ActionResult<BillDto>> MoveItem(
        [FromBody] MoveBillItemRequest request,
        CancellationToken cancellationToken)
    {
        BillDto response = await _splitBillService.MoveItemToBillAsync(request, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("session/{tableSessionId:int}/empty")]
    public async Task<ActionResult<BillDto>> CreateEmptyBill(
        int tableSessionId,
        [FromBody] CreateEmptyBillRequest request,
        CancellationToken cancellationToken)
    {
        BillDto response = await _splitBillService.CreateEmptyBillForSessionAsync(
            tableSessionId,
            request.BillName,
            CurrentUserId,
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{billId:int}")]
    public async Task<IActionResult> CancelBill(
        int billId,
        [FromBody] CancelBillRequest request,
        CancellationToken cancellationToken)
    {
        await _billService.CancelUnpaidBillAsync(billId, request.Reason, CurrentUserId, cancellationToken);
        return NoContent();
    }
}

public class CreateEmptyBillRequest
{
    public string BillName { get; set; } = string.Empty;
}

public class CancelBillRequest
{
    public string Reason { get; set; } = string.Empty;
}
