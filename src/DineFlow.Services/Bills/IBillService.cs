namespace DineFlow.Services.Bills;

public interface IBillService
{
    Task<BillDto> GetOrCreateDefaultBillAsync(int tableSessionId, int? createdBy, CancellationToken cancellationToken = default);
    Task<BillDto?> GetBillByIdAsync(int billId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BillSummaryDto>> GetBillsBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<BillDto> AddOrderItemsToDefaultBillAsync(AddOrderItemsToBillRequest request, CancellationToken cancellationToken = default);
    Task<BillDto> AdjustBillDetailQuantityAsync(AdjustBillDetailQuantityRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<BillDto> NotifyBillAsync(int billId, int currentUserId, CancellationToken cancellationToken = default);
    Task<BillDto> RenameBillAsync(int billId, string billName, CancellationToken cancellationToken = default);
    Task<BillDto> RecalculateBillTotalAsync(int billId, CancellationToken cancellationToken = default);
    Task CancelUnpaidBillAsync(int billId, string reason, int currentUserId, CancellationToken cancellationToken = default);
}
