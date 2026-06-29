using DineFlow.BusinessObjects.Bills;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Bills;

public class BillDao : IBillDao
{
    private readonly AppDbContext _dbContext;

    public BillDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Bill?> GetBillByIdAsync(int billId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Bills
            .Include(x => x.BillDetails)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.BillId == billId, cancellationToken);
    }

    public Task<Bill?> GetDefaultUnpaidBillBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Bills
            .Include(x => x.BillDetails)
            .Include(x => x.Payments)
            .Where(x => x.TableSessionId == tableSessionId && x.IsDefault && x.Status == "Unpaid")
            .OrderBy(x => x.BillNo)
            .ThenBy(x => x.BillId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Bill>> GetBillsBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bills
            .Where(x => x.TableSessionId == tableSessionId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.BillNo)
            .ThenBy(x => x.BillId)
            .ToListAsync(cancellationToken);
    }

    public Task<BillDetail?> GetBillDetailByIdAsync(int billDetailId, CancellationToken cancellationToken = default)
    {
        return _dbContext.BillDetails.FirstOrDefaultAsync(x => x.BillDetailId == billDetailId, cancellationToken);
    }

    public async Task<int> GetNextBillNoAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        int maxBillNo = await _dbContext.Bills
            .Where(x => x.TableSessionId == tableSessionId)
            .Select(x => (int?)x.BillNo)
            .MaxAsync(cancellationToken) ?? 0;

        return maxBillNo + 1;
    }

    public Task<bool> HasUnpaidBillsAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Bills.AnyAsync(x => x.TableSessionId == tableSessionId && x.Status == "Unpaid", cancellationToken);
    }

    public async Task<bool> IsSplitQuantityValidAsync(
        int billDetailId,
        int quantityToMove,
        CancellationToken cancellationToken = default)
    {
        BillDetail? detail = await _dbContext.BillDetails
            .FirstOrDefaultAsync(x => x.BillDetailId == billDetailId, cancellationToken);

        return detail is not null &&
            quantityToMove > 0 &&
            quantityToMove <= detail.Quantity;
    }

    public async Task AddBillAsync(Bill bill, CancellationToken cancellationToken = default)
    {
        await _dbContext.Bills.AddAsync(bill, cancellationToken);
    }

    public async Task ClearDefaultBillFlagsAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        List<Bill> defaultBills = await _dbContext.Bills
            .Where(x => x.TableSessionId == tableSessionId && x.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (Bill bill in defaultBills)
        {
            bill.IsDefault = false;
        }
    }

    public async Task AddBillDetailAsync(BillDetail billDetail, CancellationToken cancellationToken = default)
    {
        await _dbContext.BillDetails.AddAsync(billDetail, cancellationToken);
    }

    public async Task AddBillDetailAdjustmentAsync(BillDetailAdjustment adjustment, CancellationToken cancellationToken = default)
    {
        await _dbContext.BillDetailAdjustments.AddAsync(adjustment, cancellationToken);
    }

    public void RemoveBillDetail(BillDetail billDetail)
    {
        _dbContext.BillDetails.Remove(billDetail);
    }
}
