namespace DineFlow.Services.Bills;

public interface IPaymentCorrectionService
{
    Task<PaymentDto> UpdatePaidPaymentMethodAsync(
        int billId,
        UpdatePaidPaymentMethodRequest request,
        int currentUserId,
        string currentUserRole,
        CancellationToken cancellationToken = default);
}
