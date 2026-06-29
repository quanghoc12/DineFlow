using DineFlow.BusinessObjects.Bills;

namespace DineFlow.DataAccessObjects.Bills;

public interface IPaymentDao
{
    Task<IReadOnlyList<Payment>> GetPaymentsByBillIdAsync(int billId, CancellationToken cancellationToken = default);
    Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default);
}
