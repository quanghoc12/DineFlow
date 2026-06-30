using DineFlow.BusinessObjects.Reports;
using DineFlow.Repositories.Reports;
using DineFlow.Services.Common;

namespace DineFlow.Services.Reports;

public sealed class RevenueReportService : IRevenueReportService
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    private readonly IReportRepository _reportRepository;

    public RevenueReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public Task<RevenueSummaryDto> GetRevenueSummaryAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default)
    {
        if (fromLocalDate.Date > toLocalDate.Date)
        {
            throw new BusinessException("REVENUE_DATE_RANGE_INVALID", "Từ ngày không được lớn hơn đến ngày.");
        }

        return _reportRepository.GetRevenueSummaryByLocalDateRangeAsync(
            fromLocalDate.Date,
            toLocalDate.Date,
            VietnamOffset,
            cancellationToken);
    }

    public Task<IReadOnlyList<PaymentMethodRevenueDto>> GetRevenueByPaymentMethodAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default)
    {
        if (fromLocalDate.Date > toLocalDate.Date)
        {
            throw new BusinessException("REVENUE_PAYMENT_METHOD_DATE_RANGE_INVALID", "Từ ngày không được lớn hơn đến ngày.");
        }

        return _reportRepository.GetRevenueByPaymentMethodByLocalDateRangeAsync(
            fromLocalDate.Date,
            toLocalDate.Date,
            VietnamOffset,
            cancellationToken);
    }
}
