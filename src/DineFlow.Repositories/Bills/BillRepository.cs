using DineFlow.BusinessObjects.Bills;
using DineFlow.DataAccessObjects.Bills;

namespace DineFlow.Repositories.Bills;

public class BillRepository : IBillRepository
{
    private readonly IBillDao _billDao;

    public BillRepository(IBillDao billDao)
    {
        _billDao = billDao;
    }

    public Task<Bill?> GetBillByIdAsync(int billId, CancellationToken cancellationToken = default)
    {
        return _billDao.GetBillByIdAsync(billId, cancellationToken);
    }

    public Task<Bill?> GetDefaultUnpaidBillBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return _billDao.GetDefaultUnpaidBillBySessionAsync(tableSessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetBillsBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return await _billDao.GetBillsBySessionAsync(tableSessionId, cancellationToken);
    }

    public Task<BillDetail?> GetBillDetailByIdAsync(int billDetailId, CancellationToken cancellationToken = default)
    {
        return _billDao.GetBillDetailByIdAsync(billDetailId, cancellationToken);
    }

    public async Task<int> GetNextBillNoAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return await _billDao.GetNextBillNoAsync(tableSessionId, cancellationToken);
    }

    public Task<bool> HasUnpaidBillsAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return _billDao.HasUnpaidBillsAsync(tableSessionId, cancellationToken);
    }

    public async Task<bool> IsSplitQuantityValidAsync(
        int billDetailId,
        int quantityToMove,
        CancellationToken cancellationToken = default)
    {
        return await _billDao.IsSplitQuantityValidAsync(billDetailId, quantityToMove, cancellationToken);
    }

    public Task ClearDefaultBillFlagsAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return _billDao.ClearDefaultBillFlagsAsync(tableSessionId, cancellationToken);
    }

    public async Task AddBillAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        await _billDao.AddBillAsync(bill, cancellationToken);
    }

    public async Task AddBillDetailAsync(BillDetail billDetail, CancellationToken cancellationToken = default)
    {
        await _billDao.AddBillDetailAsync(billDetail, cancellationToken);
    }

    public async Task AddBillDetailAdjustmentAsync(BillDetailAdjustment adjustment, CancellationToken cancellationToken = default)
    {
        await _billDao.AddBillDetailAdjustmentAsync(adjustment, cancellationToken);
    }

    public void RemoveBillDetail(BillDetail billDetail)
    {
        _billDao.RemoveBillDetail(billDetail);
    }
}
