using DineFlow.Services.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Customer;

[ApiController]
[Route("api/customer/service-requests")]
public class CustomerServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;

    public CustomerServiceRequestsController(IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    [HttpPost("call-staff")]
    public async Task<ActionResult<ServiceRequestDto>> CallStaff(
        [FromBody] CreateServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        request.RequestType = "CallStaff";
        request.PaymentMethod = null;

        ServiceRequestDto response = await _serviceRequestService.CreateServiceRequestAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("payment-request")]
    public async Task<ActionResult<ServiceRequestDto>> PaymentRequest(
        [FromBody] CreateServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        request.RequestType = "PaymentRequest";

        ServiceRequestDto response = await _serviceRequestService.CreateServiceRequestAsync(request, cancellationToken);
        return Ok(response);
    }
}
