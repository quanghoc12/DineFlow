using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Bills;
using DineFlow.Repositories.Bills;
using DineFlow.Repositories.Common;
using DineFlow.Services.Common;

namespace DineFlow.Services.Bills;

public sealed class PaymentCorrectionService : IPaymentCorrectionService
{
    private readonly IBillRepository _billRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentCorrectionService(
        IBillRepository billRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentDto> UpdatePaidPaymentMethodAsync(
        int billId,
        UpdatePaidPaymentMethodRequest request,
        int currentUserId,
        string currentUserRole,
        CancellationToken cancellationToken = default)
    {
        if (!AuthRoles.CanManage(currentUserRole))
        {
            throw new UnauthorizedAccessException("Chỉ Admin hoặc Chủ nhà hàng được sửa phương thức thanh toán.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill bill = await _billRepository.GetBillByIdAsync(billId, ct)
                ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

            if (bill.Status != "Paid")
            {
                throw new BusinessException("BILL_NOT_PAID", "Chỉ bill đã thanh toán mới được sửa payment method.");
            }

            IReadOnlyList<Payment> payments = await _paymentRepository.GetPaymentsByBillIdAsync(billId, ct);
            Payment payment = payments.FirstOrDefault(x => x.PaymentId == request.PaymentId)
                ?? throw new BusinessException("PAYMENT_NOT_FOUND", "Payment không thuộc bill được chọn.");

            string normalizedMethod = request.NewPaymentMethod.Trim();
            if (!PaymentMethods.IsStoredValue(normalizedMethod))
            {
                throw new BusinessException("PAYMENT_METHOD_INVALID", "Payment method is invalid.");
            }

            string changeReason = request.ChangeReason.Trim();
            if (string.IsNullOrWhiteSpace(changeReason))
            {
                throw new BusinessException("CHANGE_REASON_REQUIRED", "Lý do thay đổi là bắt buộc.");
            }

            payment.PaymentMethod = normalizedMethod;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.UpdatedBy = currentUserId;
            payment.ChangeReason = changeReason;

            await _unitOfWork.SaveChangesAsync(ct);
            return MapPayment(payment);
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentDto>> BatchUpdatePaidPaymentsAsync(
        int billId,
        BatchUpdatePaymentsRequest request,
        int currentUserId,
        string currentUserRole,
        CancellationToken cancellationToken = default)
    {
        if (!AuthRoles.CanManage(currentUserRole))
        {
            throw new UnauthorizedAccessException("Chỉ Admin hoặc Chủ nhà hàng được sửa phương thức thanh toán.");
        }

        string changeReason = request.ChangeReason.Trim();
        if (string.IsNullOrWhiteSpace(changeReason))
        {
            throw new BusinessException("CHANGE_REASON_REQUIRED", "Lý do thay đổi là bắt buộc.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill bill = await _billRepository.GetBillByIdAsync(billId, ct)
                ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

            if (bill.Status != "Paid")
            {
                throw new BusinessException("BILL_NOT_PAID", "Chỉ bill đã thanh toán mới được sửa payment.");
            }

            IReadOnlyList<Payment> payments = await _paymentRepository.GetPaymentsByBillIdAsync(billId, ct);
            
            decimal newSum = request.Payments.Sum(x => x.Amount);
            if (newSum != bill.FinalAmount)
            {
                throw new BusinessException("PAYMENT_TOTAL_MISMATCH", $"Tổng số tiền thanh toán ({newSum:N0} đ) phải bằng số tiền hóa đơn ({bill.FinalAmount:N0} đ).");
            }

            List<PaymentDto> updatedDtos = [];
            DateTime updateTime = DateTime.UtcNow;

            foreach (var updatePart in request.Payments)
            {
                Payment payment = payments.FirstOrDefault(x => x.PaymentId == updatePart.PaymentId)
                    ?? throw new BusinessException("PAYMENT_NOT_FOUND", $"Payment ID {updatePart.PaymentId} không thuộc bill.");

                string normalizedMethod = updatePart.PaymentMethod.Trim();
                if (!PaymentMethods.IsStoredValue(normalizedMethod))
                {
                    throw new BusinessException("PAYMENT_METHOD_INVALID", $"Phương thức {updatePart.PaymentMethod} không hợp lệ.");
                }

                if (updatePart.Amount < 0)
                {
                    throw new BusinessException("PAYMENT_AMOUNT_INVALID", "Số tiền thanh toán phải lớn hơn hoặc bằng 0.");
                }

                payment.PaymentMethod = normalizedMethod;
                payment.Amount = updatePart.Amount;
                payment.UpdatedAt = updateTime;
                payment.UpdatedBy = currentUserId;
                payment.ChangeReason = changeReason;

                updatedDtos.Add(MapPayment(payment));
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return updatedDtos;
        }, cancellationToken);
    }

    private static PaymentDto MapPayment(Payment payment)
    {
        return new PaymentDto
        {
            PaymentId = payment.PaymentId,
            BillId = payment.BillId,
            PaymentMethod = payment.PaymentMethod,
            Amount = payment.Amount,
            PaidAt = payment.PaidAt,
            ConfirmedBy = payment.ConfirmedBy,
            UpdatedAt = payment.UpdatedAt,
            UpdatedBy = payment.UpdatedBy,
            ChangeReason = payment.ChangeReason
        };
    }
}
