namespace DineFlow.Services.Bills;

public interface IPaymentService
{
    Task<PaymentDto> ConfirmPaymentAsync(ConfirmPaymentRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<PaymentResultDto> ConfirmCombinedPaymentAsync(ConfirmCombinedPaymentRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentDto>> GetPaymentsByBillIdAsync(int billId, CancellationToken cancellationToken = default);
    Task<bool> HasUnpaidBillsAsync(int tableSessionId, CancellationToken cancellationToken = default);
}
