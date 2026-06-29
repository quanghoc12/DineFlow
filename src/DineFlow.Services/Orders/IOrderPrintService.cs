namespace DineFlow.Services.Orders;

public interface IOrderPrintService
{
    Task MarkPrintedAsync(int orderId, int currentUserId, CancellationToken cancellationToken = default);
    Task MarkPrintFailedAsync(int orderId, MarkPrintFailedRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task RequestReprintAsync(int orderId, int currentUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderSummaryDto>> GetWaitingPrintOrdersAsync(CancellationToken cancellationToken = default);
}
