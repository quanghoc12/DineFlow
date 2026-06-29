using DineFlow.BusinessObjects.Bills;
using DineFlow.DataAccessObjects.Bills;

namespace DineFlow.Repositories.Bills;

public class PaymentRepository : IPaymentRepository
{
    private readonly IPaymentDao _paymentDao;

    public PaymentRepository(IPaymentDao paymentDao)
    {
        _paymentDao = paymentDao;
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsByBillIdAsync(
        int billId,
        CancellationToken cancellationToken = default)
    {
        return await _paymentDao.GetPaymentsByBillIdAsync(billId, cancellationToken);
    }

    public async Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _paymentDao.AddPaymentAsync(payment, cancellationToken);
    }
}
