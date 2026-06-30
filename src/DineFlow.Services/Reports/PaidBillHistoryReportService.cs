using DineFlow.BusinessObjects.Reports;
using DineFlow.Repositories.Reports;
using DineFlow.Services.Common;

namespace DineFlow.Services.Reports;

public sealed class PaidBillHistoryReportService : IPaidBillHistoryReportService
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    private readonly IReportRepository _reportRepository;

    public PaidBillHistoryReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public Task<IReadOnlyList<PaidBillHistoryItemDto>> GetPaidBillHistoryAsync(
        PaidBillHistoryFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        DateTime fromDate = filter.FromDate.Date;
        DateTime toDate = filter.ToDate.Date;

        if (fromDate > toDate)
        {
            throw new BusinessException("PAID_BILL_HISTORY_DATE_RANGE_INVALID", "Từ ngày không được lớn hơn đến ngày.");
        }

        string? paymentMethod = Normalize(filter.PaymentMethod);
        if (!string.IsNullOrEmpty(paymentMethod) &&
            !string.Equals(paymentMethod, "All", StringComparison.OrdinalIgnoreCase))
        {
            string[] allowedMethods = ["Cash", "BankTransfer", "Card"];
            if (!allowedMethods.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase))
            {
                throw new BusinessException("PAID_BILL_HISTORY_PAYMENT_METHOD_INVALID", "Phương thức thanh toán không hợp lệ.");
            }
        }

        string? keyword = Normalize(filter.Keyword);

        return _reportRepository.GetPaidBillHistoryByLocalDateRangeAsync(
            fromDate,
            toDate,
            VietnamOffset,
            string.Equals(paymentMethod, "All", StringComparison.OrdinalIgnoreCase) ? null : paymentMethod,
            Normalize(filter.TableName),
            Normalize(filter.Area),
            keyword,
            cancellationToken);
    }

    private static string? Normalize(string? value)
    {
        string? normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
