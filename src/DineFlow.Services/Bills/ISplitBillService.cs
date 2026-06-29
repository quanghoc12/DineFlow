namespace DineFlow.Services.Bills;

public interface ISplitBillService
{
    Task<BillDto> CreateSplitBillAsync(SplitBillRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<BillDto> SplitBillBatchAsync(SplitBillBatchRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<BillDto> MergeBillAsync(MergeBillRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<BillDto> MoveItemToBillAsync(MoveBillItemRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<BillDto> CreateEmptyBillForSessionAsync(int tableSessionId, string billName, int currentUserId, CancellationToken cancellationToken = default);
    Task<bool> ValidateSplitQuantityAsync(int billDetailId, int quantityToMove, CancellationToken cancellationToken = default);
}
