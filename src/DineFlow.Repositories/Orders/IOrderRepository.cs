using DineFlow.BusinessObjects.Orders;

namespace DineFlow.Repositories.Orders;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetOrdersAsync(
        int? tableSessionId,
        string? status,
        string? printStatus,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetWaitingPrintOrdersAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task AddOrderAsync(Order order, CancellationToken cancellationToken = default);
    Task AddOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default);
    Task AddSelectedChoiceAsync(OrderItemSelectedChoice selectedChoice, CancellationToken cancellationToken = default);
}
