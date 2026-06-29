using DineFlow.Services.Bills;

namespace DineFlow.Services.Orders;

public interface IOrderService
{
    Task<CreateOrderResponse> CreateCustomerOrderAsync(CreateCustomerOrderRequest request, CancellationToken cancellationToken = default);
    Task<CreateOrderResponse> CreateStaffOrderAsync(CreateStaffOrderRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<BillDto> ConfirmOrderAsync(int orderId, ConfirmOrderRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<OrderDetailDto> CancelPendingOrderAsync(int orderId, CancelOrderRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(OrderFilter filter, CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> GetOrderDetailAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderSummaryDto>> GetOrdersBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task SystemCancelOrderBeforeBillMergeAsync(int orderId, string systemReason, CancellationToken cancellationToken = default);
}
