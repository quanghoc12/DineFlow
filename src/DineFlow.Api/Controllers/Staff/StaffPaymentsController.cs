using DineFlow.Services.Bills;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/payments")]
public class StaffPaymentsController : StaffControllerBase
{
    private readonly IPaymentService _paymentService;

    public StaffPaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("confirm-combined")]
    public async Task<ActionResult<PaymentResultDto>> ConfirmCombined(
        [FromBody] ConfirmCombinedPaymentRequest request,
        CancellationToken cancellationToken)
    {
        PaymentResultDto response = await _paymentService.ConfirmCombinedPaymentAsync(
            request,
            CurrentUserId,
            cancellationToken);
        return Ok(response);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<PaymentDto>> Confirm(
        [FromBody] ConfirmPaymentRequest request,
        CancellationToken cancellationToken)
    {
        PaymentDto response = await _paymentService.ConfirmPaymentAsync(request, CurrentUserId, cancellationToken);
        return Ok(response);
    }

    [HttpGet("bill/{billId:int}")]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> GetByBill(
        int billId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<PaymentDto> response = await _paymentService.GetPaymentsByBillIdAsync(billId, cancellationToken);
        return Ok(response);
    }
}
