using DineFlow.Services.Bills;
using DineFlow.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/orders")]
public class StaffOrdersController : StaffControllerBase
{
    private readonly IOrderPrintService _orderPrintService;
    private readonly IOrderService _orderService;

    public StaffOrdersController(IOrderPrintService orderPrintService, IOrderService orderService)
    {
        _orderPrintService = orderPrintService;
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetOrders(
        [FromQuery] int? tableSessionId,
        [FromQuery] string? status,
        [FromQuery] string? printStatus,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<OrderSummaryDto> response = await _orderService.GetOrdersAsync(new OrderFilter
        {
            TableSessionId = tableSessionId,
            Status = status,
            PrintStatus = printStatus,
            From = from,
            To = to
        }, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(int id, CancellationToken cancellationToken)
    {
        OrderDetailDto? response = await _orderService.GetOrderDetailAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(
        [FromBody] CreateStaffOrderRequest request,
        CancellationToken cancellationToken)
    {
        CreateOrderResponse response = await _orderService.CreateStaffOrderAsync(request, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<ActionResult<BillDto>> ConfirmOrder(
        int id,
        [FromBody] ConfirmOrderRequest request,
        CancellationToken cancellationToken)
    {
        BillDto response = await _orderService.ConfirmOrderAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<OrderDetailDto>> CancelOrder(
        int id,
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        OrderDetailDto response = await _orderService.CancelPendingOrderAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("waiting-print")]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetWaitingPrint(CancellationToken cancellationToken)
    {
        IReadOnlyList<OrderSummaryDto> response = await _orderPrintService.GetWaitingPrintOrdersAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPut("{id:int}/mark-printed")]
    public async Task<IActionResult> MarkPrinted(int id, CancellationToken cancellationToken)
    {
        await _orderPrintService.MarkPrintedAsync(id, CurrentUserId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/mark-print-failed")]
    public async Task<IActionResult> MarkPrintFailed(
        int id,
        [FromBody] MarkPrintFailedRequest request,
        CancellationToken cancellationToken)
    {
        await _orderPrintService.MarkPrintFailedAsync(id, request, CurrentUserId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/reprint")]
    public async Task<IActionResult> Reprint(int id, CancellationToken cancellationToken)
    {
        await _orderPrintService.RequestReprintAsync(id, CurrentUserId, cancellationToken);
        return NoContent();
    }
}
