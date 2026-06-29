using DineFlow.Services.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/requests")]
public class StaffRequestsController : StaffControllerBase
{
    private readonly IServiceRequestService _serviceRequestService;

    public StaffRequestsController(IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceRequestDto>>> GetConfirmed(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        if (from.HasValue || to.HasValue || !string.IsNullOrWhiteSpace(status))
        {
            IReadOnlyList<ServiceRequestDto> requests = await _serviceRequestService.GetRequestsAsync(from, to, status, cancellationToken);
            return Ok(requests);
        }

        IReadOnlyList<ServiceRequestDto> response = await _serviceRequestService.GetConfirmedRequestsAsync(cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<ActionResult<ServiceRequestDto>> ConfirmRequest(int id, CancellationToken cancellationToken)
    {
        ServiceRequestDto response = await _serviceRequestService.ConfirmRequestAsync(id, CurrentUserId, cancellationToken);
        return Ok(response);
    }
}
