using DineFlow.Services.CustomerSessions;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Customer;

[ApiController]
[Route("api/customer/messages")]
public class CustomerMessagesController : ControllerBase
{
    private readonly ICustomerSessionService _customerSessionService;

    public CustomerMessagesController(ICustomerSessionService customerSessionService)
    {
        _customerSessionService = customerSessionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerMessageDto>>> GetMessages(
        [FromQuery] string clientToken,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CustomerMessageDto> response = await _customerSessionService.GetMessagesAsync(clientToken, cancellationToken);
        return Ok(response);
    }
}
