using DineFlow.BusinessObjects.Orders;
using DineFlow.Repositories.Common;
using DineFlow.Repositories.Orders;
using DineFlow.Services.Common;

namespace DineFlow.Services.Orders;

public class OrderPrintService : IOrderPrintService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderPrintService(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task MarkPrintedAsync(int orderId, int currentUserId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Order order = await _orderRepository.GetOrderByIdAsync(orderId, ct)
                ?? throw new BusinessException("ORDER_NOT_FOUND", "Order does not exist.");

            order.PrintStatus = "Printed";
            order.PrintedAt = DateTime.UtcNow;
            order.PrintError = null;
            order.UpdatedAt = DateTime.UtcNow;
        }, cancellationToken);
    }

    public async Task MarkPrintFailedAsync(
        int orderId,
        MarkPrintFailedRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Order order = await _orderRepository.GetOrderByIdAsync(orderId, ct)
                ?? throw new BusinessException("ORDER_NOT_FOUND", "Order does not exist.");

            order.PrintStatus = "PrintFailed";
            order.PrintError = request.PrintError;
            order.UpdatedAt = DateTime.UtcNow;
        }, cancellationToken);
    }

    public async Task RequestReprintAsync(int orderId, int currentUserId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Order order = await _orderRepository.GetOrderByIdAsync(orderId, ct)
                ?? throw new BusinessException("ORDER_NOT_FOUND", "Order does not exist.");

            if (order.PrintStatus != "Printed" && order.PrintStatus != "PrintFailed")
            {
                throw new BusinessException("REPRINT_NOT_ALLOWED", "Only printed or failed orders can be reprinted.");
            }

            order.PrintStatus = null;
            order.PrintError = null;
            order.UpdatedAt = DateTime.UtcNow;
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetWaitingPrintOrdersAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OrderSummaryDto> orders = (await _orderRepository.GetWaitingPrintOrdersAsync(cancellationToken))
            .Select(x => new OrderSummaryDto
            {
                OrderId = x.OrderId,
                OrderCode = x.OrderCode,
                TableSessionId = x.TableSessionId,
                OrderSource = x.OrderSource,
                Status = x.Status,
                PrintStatus = x.PrintStatus,
                CreatedAt = x.CreatedAt,
                ItemCount = x.OrderItems.Sum(item => item.Quantity)
            })
            .ToList();

        return orders;
    }
}
