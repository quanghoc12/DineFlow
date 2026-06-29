using DineFlow.BusinessObjects.Orders;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Orders;

public class OrderDao : IOrderDao
{
    private readonly AppDbContext _dbContext;

    public OrderDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersAsync(
        int? tableSessionId,
        string? status,
        string? printStatus,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = _dbContext.Orders
            .Include(x => x.SalesChannel)
            .Include(x => x.TableSession)
                .ThenInclude(x => x!.Table)
            .Include(x => x.OrderItems);

        if (tableSessionId.HasValue)
        {
            query = query.Where(x => x.TableSessionId == tableSessionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(printStatus))
        {
            query = query.Where(x => x.PrintStatus == printStatus);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= DateTime.SpecifyKind(from.Value, DateTimeKind.Utc));
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersBySessionAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(x => x.SalesChannel)
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.SelectedChoices)
            .Where(x => x.TableSessionId == tableSessionId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetWaitingPrintOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
            .Include(x => x.SalesChannel)
            .Include(x => x.OrderItems)
            .Where(x => x.Status == "Accepted" && x.PrintStatus == null)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Orders
            .Include(x => x.SalesChannel)
            .Include(x => x.TableSession)
                .ThenInclude(x => x!.Table)
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.SelectedChoices)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
    }

    public async Task AddOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public async Task AddOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
    {
        await _dbContext.OrderItems.AddAsync(orderItem, cancellationToken);
    }

    public async Task AddSelectedChoiceAsync(OrderItemSelectedChoice selectedChoice, CancellationToken cancellationToken = default)
    {
        await _dbContext.OrderItemSelectedChoices.AddAsync(selectedChoice, cancellationToken);
    }
}
