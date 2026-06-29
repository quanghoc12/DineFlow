using DineFlow.BusinessObjects.Bills;

namespace DineFlow.DataAccessObjects.Bills;

public interface IBillDao
{
    Task<Bill?> GetBillByIdAsync(int billId, CancellationToken cancellationToken = default);
    Task<Bill?> GetDefaultUnpaidBillBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetBillsBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<BillDetail?> GetBillDetailByIdAsync(int billDetailId, CancellationToken cancellationToken = default);
    Task<int> GetNextBillNoAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<bool> HasUnpaidBillsAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<bool> IsSplitQuantityValidAsync(int billDetailId, int quantityToMove, CancellationToken cancellationToken = default);
    Task ClearDefaultBillFlagsAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task AddBillAsync(Bill bill, CancellationToken cancellationToken = default);
    Task AddBillDetailAsync(BillDetail billDetail, CancellationToken cancellationToken = default);
    Task AddBillDetailAdjustmentAsync(BillDetailAdjustment adjustment, CancellationToken cancellationToken = default);
    void RemoveBillDetail(BillDetail billDetail);
}
