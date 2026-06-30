using DineFlow.Api.Controllers.Staff;
using DineFlow.Services.Bills;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/payments")]
public sealed class AdminPaymentsController : StaffControllerBase
{
    private readonly IPaymentCorrectionService _paymentCorrectionService;

    public AdminPaymentsController(IPaymentCorrectionService paymentCorrectionService)
    {
        _paymentCorrectionService = paymentCorrectionService;
    }

    [HttpPut("{billId:int}/method")]
    public async Task<ActionResult<PaymentDto>> UpdatePaidPaymentMethod(
        int billId,
        [FromBody] UpdatePaidPaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        PaymentDto response = await _paymentCorrectionService.UpdatePaidPaymentMethodAsync(
            billId,
            request,
            CurrentUserId,
            CurrentUserRole,
            cancellationToken);

        return Ok(response);
    }
}
