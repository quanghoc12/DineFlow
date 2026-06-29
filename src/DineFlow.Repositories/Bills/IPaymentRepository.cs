using DineFlow.BusinessObjects.Bills;

namespace DineFlow.Repositories.Bills;

public interface IPaymentRepository
{
    Task<IReadOnlyList<Payment>> GetPaymentsByBillIdAsync(int billId, CancellationToken cancellationToken = default);
    Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default);
}
