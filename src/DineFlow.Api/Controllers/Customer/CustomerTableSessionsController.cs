using DineFlow.Services.CustomerSessions;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Customer;

[ApiController]
[Route("api/customer/table-sessions")]
public class CustomerTableSessionsController : ControllerBase
{
    private readonly ICustomerSessionService _customerSessionService;

    public CustomerTableSessionsController(ICustomerSessionService customerSessionService)
    {
        _customerSessionService = customerSessionService;
    }

    [HttpPost("scan")]
    public async Task<ActionResult<CustomerSessionDto>> Scan(
        [FromBody] ScanCustomerSessionRequest request,
        CancellationToken cancellationToken)
    {
        CustomerSessionDto response = await _customerSessionService.ScanAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("customer-name")]
    public async Task<ActionResult<CustomerSessionDto>> UpdateCustomerName(
        [FromBody] UpdateCustomerNameRequest request,
        CancellationToken cancellationToken)
    {
        CustomerSessionDto response = await _customerSessionService.UpdateCustomerNameAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    public async Task<ActionResult<CustomerSessionDto>> Me(
        [FromQuery] string clientToken,
        CancellationToken cancellationToken)
    {
        CustomerSessionDto response = await _customerSessionService.GetCurrentAsync(clientToken, cancellationToken);
        return Ok(response);
    }
}
