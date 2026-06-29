using DineFlow.BusinessObjects.Bills;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Bills;

public class PaymentDao : IPaymentDao
{
    private readonly AppDbContext _dbContext;

    public PaymentDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsByBillIdAsync(
        int billId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .Where(x => x.BillId == billId)
            .OrderBy(x => x.PaidAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Payments.AddAsync(payment, cancellationToken);
    }
}
