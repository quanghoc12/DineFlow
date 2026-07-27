using DineFlow.BusinessObjects.Bills;
using DineFlow.Repositories.Bills;
using DineFlow.Repositories.Common;
using DineFlow.Repositories.Orders;
using DineFlow.Services.Common;
using DineFlow.Services.Realtime;
using DineFlow.Services.Tables;

namespace DineFlow.Services.Bills;

public class PaymentService : IPaymentService
{
    private readonly IBillRepository _billRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ITableSessionRepository _tableSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IBillRepository billRepository,
        IPaymentRepository paymentRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ITableSessionRepository tableSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _paymentRepository = paymentRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _tableSessionRepository = tableSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentResultDto> ConfirmCombinedPaymentAsync(
        ConfirmCombinedPaymentRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        RealtimeEventDto? tableOtpChangedPayload = null;
        PaymentResultDto result = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill bill = await _billRepository.GetBillByIdAsync(request.BillId, ct)
                ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

            if (bill.Status != "Unpaid")
            {
                throw new BusinessException("BILL_NOT_UNPAID", "Only unpaid bill can receive payment.");
            }

            if (bill.BillDetails.Count == 0)
            {
                throw new BusinessException("BILL_EMPTY", "Bill must have at least one detail before payment.");
            }

            List<PaymentPartRequest> parts = request.Payments
                .Where(x => x.Amount > 0)
                .ToList();

            if (parts.Count == 0 ||
                parts.Any(x => !PaymentMethods.IsStoredValue(x.PaymentMethod)))
            {
                throw new BusinessException("PAYMENT_INVALID", "Payment methods and amounts are invalid.");
            }

            decimal alreadyPaid = bill.Payments.Sum(x => x.Amount);
            decimal remaining = bill.FinalAmount - alreadyPaid;
            decimal submitted = parts.Sum(x => x.Amount);

            if (submitted != remaining)
            {
                throw new BusinessException(
                    "PAYMENT_TOTAL_MISMATCH",
                    $"Payment total must equal the remaining amount ({remaining:N0}).");
            }

            DateTime paidAt = DateTime.UtcNow;
            List<Payment> payments = [];
            foreach (PaymentPartRequest part in parts)
            {
                var payment = new Payment
                {
                    BillId = bill.BillId,
                    PaymentMethod = part.PaymentMethod,
                    Amount = part.Amount,
                    PaidAt = paidAt,
                    ConfirmedBy = currentUserId
                };
                await _paymentRepository.AddPaymentAsync(payment, ct);
                payments.Add(payment);
            }

            bill.Status = "Paid";
            bill.PaidAt = paidAt;
            await _unitOfWork.SaveChangesAsync(ct);

            bool sessionClosed = false;
            if (!await _billRepository.HasUnpaidBillsAsync(bill.TableSessionId, ct))
            {
                var session = await _tableSessionRepository.GetByIdAsync(bill.TableSessionId, ct);
                if (session is not null)
                {
                    session.Status = "Closed";
                    session.EndedAt = paidAt;
                    session.ClosedBy = currentUserId;

                    var table = await _tableSessionRepository.GetTableByIdAsync(session.TableId, ct);
                    if (table is not null)
                    {
                        table.Status = "Available";
                        TableOtpRotation.Rotate(table, paidAt);
                        tableOtpChangedPayload = CreateTableOtpChangedPayload(table, session);
                    }

                    sessionClosed = true;
                    await _unitOfWork.SaveChangesAsync(ct);
                }
            }

            return new PaymentResultDto
            {
                BillId = bill.BillId,
                BillStatus = bill.Status,
                TotalAmount = bill.FinalAmount,
                PaidAmount = alreadyPaid + submitted,
                SessionClosed = sessionClosed,
                Payments = payments.Select(MapPayment).ToList()
            };
        }, cancellationToken);

        await NotifyPaymentChangedAsync(result.BillId, result.SessionClosed, cancellationToken);
        if (tableOtpChangedPayload is not null)
        {
            await NotifyTableOtpChangedAsync(tableOtpChangedPayload, cancellationToken);
        }
        return result;
    }

    public async Task<PaymentDto> ConfirmPaymentAsync(
        ConfirmPaymentRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        bool sessionClosed = false;
        RealtimeEventDto? tableOtpChangedPayload = null;
        PaymentDto dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill bill = await _billRepository.GetBillByIdAsync(request.BillId, ct)
                ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

            if (bill.Status != "Unpaid")
            {
                throw new BusinessException("BILL_NOT_UNPAID", "Only unpaid bill can receive payment.");
            }

            if (bill.BillDetails.Count == 0)
            {
                throw new BusinessException("BILL_EMPTY", "Bill must have at least one detail before payment.");
            }

            if (!PaymentMethods.IsStoredValue(request.PaymentMethod))
            {
                throw new BusinessException("PAYMENT_METHOD_INVALID", "Payment method is invalid.");
            }

            if (request.Amount <= 0)
            {
                throw new BusinessException("PAYMENT_AMOUNT_INVALID", "Payment amount must be greater than zero.");
            }

            decimal paidAmount = bill.Payments.Sum(x => x.Amount);
            decimal remainingAmount = bill.FinalAmount - paidAmount;

            if (request.Amount > remainingAmount)
            {
                throw new BusinessException("PAYMENT_AMOUNT_EXCEEDS_REMAINING", "Payment amount exceeds remaining bill amount.");
            }

            var payment = new Payment
            {
                BillId = bill.BillId,
                PaymentMethod = request.PaymentMethod,
                Amount = request.Amount,
                PaidAt = DateTime.UtcNow,
                ConfirmedBy = currentUserId
            };

            await _paymentRepository.AddPaymentAsync(payment, ct);

            if (paidAmount + request.Amount == bill.FinalAmount)
            {
                bill.Status = "Paid";
                bill.PaidAt = payment.PaidAt;
            }

            await _unitOfWork.SaveChangesAsync(ct);

            if (bill.Status == "Paid" && !await _billRepository.HasUnpaidBillsAsync(bill.TableSessionId, ct))
            {
                var session = await _tableSessionRepository.GetByIdAsync(bill.TableSessionId, ct);
                if (session is not null)
                {
                    session.Status = "Closed";
                    session.EndedAt = payment.PaidAt;
                    session.ClosedBy = currentUserId;

                    var table = await _tableSessionRepository.GetTableByIdAsync(session.TableId, ct);
                    if (table is not null)
                    {
                        table.Status = "Available";
                        TableOtpRotation.Rotate(table, payment.PaidAt);
                        tableOtpChangedPayload = CreateTableOtpChangedPayload(table, session);
                    }

                    sessionClosed = true;
                    await _unitOfWork.SaveChangesAsync(ct);
                }
            }

            return MapPayment(payment);
        }, cancellationToken);

        await NotifyPaymentChangedAsync(dto.BillId, sessionClosed, cancellationToken);
        if (tableOtpChangedPayload is not null)
        {
            await NotifyTableOtpChangedAsync(tableOtpChangedPayload, cancellationToken);
        }
        return dto;
    }

    public async Task<IReadOnlyList<PaymentDto>> GetPaymentsByBillIdAsync(int billId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PaymentDto> payments = (await _paymentRepository.GetPaymentsByBillIdAsync(billId, cancellationToken))
            .Select(MapPayment)
            .ToList();

        return payments;
    }

    public async Task<bool> HasUnpaidBillsAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        return await _billRepository.HasUnpaidBillsAsync(tableSessionId, cancellationToken);
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

    private static RealtimeEventDto CreateTableOtpChangedPayload(
        DineFlow.BusinessObjects.Tables.DiningTable table,
        DineFlow.BusinessObjects.Orders.TableSession? session = null)
    {
        return new RealtimeEventDto
        {
            TableSessionId = session?.TableSessionId ?? 0,
            TableId = table.TableId,
            CurrentOtp = table.CurrentOtp,
            OtpUpdatedAt = table.OtpUpdatedAt,
            TableStatus = table.Status,
            SessionStatus = session?.Status
        };
    }

    private async Task NotifyTableOtpChangedAsync(
        RealtimeEventDto payload,
        CancellationToken cancellationToken)
    {
        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.TableOtpChanged,
            payload,
            cancellationToken);

        if (payload.TableSessionId > 0)
        {
            await _realtimeNotificationService.NotifySessionAsync(
                payload.TableSessionId,
                RealtimeEvents.TableOtpChanged,
                payload,
                cancellationToken);
        }
    }

    private async Task NotifyPaymentChangedAsync(
        int billId,
        bool sessionClosed,
        CancellationToken cancellationToken)
    {
        Bill? bill = await _billRepository.GetBillByIdAsync(billId, cancellationToken);
        if (bill is null)
        {
            return;
        }

        RealtimeEventDto payload = new()
        {
            TableSessionId = bill.TableSessionId,
            BillId = bill.BillId
        };

        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.PaymentConfirmed,
            payload,
            cancellationToken);
        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.BillChanged,
            payload,
            cancellationToken);
        await _realtimeNotificationService.NotifySessionAsync(
            bill.TableSessionId,
            RealtimeEvents.BillChanged,
            payload,
            cancellationToken);

        if (sessionClosed)
        {
            await _realtimeNotificationService.NotifyStaffAsync(
                RealtimeEvents.TableSessionChanged,
                payload,
                cancellationToken);
            await _realtimeNotificationService.NotifySessionAsync(
                bill.TableSessionId,
                RealtimeEvents.TableSessionChanged,
                payload,
                cancellationToken);
        }
    }
}
