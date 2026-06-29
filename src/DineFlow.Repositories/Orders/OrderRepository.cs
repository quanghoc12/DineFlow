using DineFlow.BusinessObjects.Orders;
using DineFlow.DataAccessObjects.Orders;

namespace DineFlow.Repositories.Orders;

public class OrderRepository : IOrderRepository
{
    private readonly IOrderDao _orderDao;

    public OrderRepository(IOrderDao orderDao)
    {
        _orderDao = orderDao;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(
        int? tableSessionId,
        string? status,
        string? printStatus,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        return await _orderDao.GetOrdersAsync(tableSessionId, status, printStatus, from, to, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersBySessionAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _orderDao.GetOrdersBySessionAsync(tableSessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetWaitingPrintOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _orderDao.GetWaitingPrintOrdersAsync(cancellationToken);
    }

    public Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return _orderDao.GetOrderByIdAsync(orderId, cancellationToken);
    }

    public async Task AddOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _orderDao.AddOrderAsync(order, cancellationToken);
    }

    public async Task AddOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        await _orderDao.AddOrderItemAsync(orderItem, cancellationToken);
    }

    public async Task AddSelectedChoiceAsync(OrderItemSelectedChoice selectedChoice, CancellationToken cancellationToken = default)
    {
        await _orderDao.AddSelectedChoiceAsync(selectedChoice, cancellationToken);
    }
}
