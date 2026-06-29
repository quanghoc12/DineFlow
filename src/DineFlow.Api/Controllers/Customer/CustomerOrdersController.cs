using DineFlow.Services.Orders;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Customer;

[ApiController]
[Route("api/customer/orders")]
public class CustomerOrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public CustomerOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(
        [FromBody] CreateCustomerOrderRequest request,
        CancellationToken cancellationToken)
    {
        CreateOrderResponse response = await _orderService.CreateCustomerOrderAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("{orderCode}")]
    public async Task<ActionResult<OrderDetailDto>> GetByOrderCode(
        string orderCode,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<OrderSummaryDto> orders = await _orderService.GetOrdersAsync(new OrderFilter(), cancellationToken);
        OrderSummaryDto? order = orders.FirstOrDefault(x => x.OrderCode == orderCode);

        if (order is null)
        {
            return NotFound();
        }

        OrderDetailDto? detail = await _orderService.GetOrderDetailAsync(order.OrderId, cancellationToken);
        return detail is null ? NotFound() : Ok(detail);
    }
}
