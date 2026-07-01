using DineFlow.BusinessObjects.Reports;
using DineFlow.DataAccessObjects.Reports;

namespace DineFlow.Repositories.Reports;

public sealed class ReportRepository : IReportRepository
{
    private readonly IReportDao _reportDao;

    public ReportRepository(IReportDao reportDao)
    {
        _reportDao = reportDao;
    }

    public Task<DashboardDto> GetDashboardByLocalDateAsync(
        DateTime localDate,
        TimeSpan localOffset,
        int topItemCount,
        CancellationToken cancellationToken = default) =>
        _reportDao.GetDashboardByLocalDateAsync(localDate, localOffset, topItemCount, cancellationToken);

    public Task<RevenueSummaryDto> GetRevenueSummaryByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        CancellationToken cancellationToken = default) =>
        _reportDao.GetRevenueSummaryByLocalDateRangeAsync(
            fromLocalDate,
            toLocalDate,
            localOffset,
            cancellationToken);

    public Task<IReadOnlyList<TopSellingItemDto>> GetTopSellingItemsByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        int topCount,
        CancellationToken cancellationToken = default) =>
        _reportDao.GetTopSellingItemsByLocalDateRangeAsync(
            fromLocalDate,
            toLocalDate,
            localOffset,
            topCount,
            cancellationToken);

    public Task<IReadOnlyList<PaymentMethodRevenueDto>> GetRevenueByPaymentMethodByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        CancellationToken cancellationToken = default) =>
        _reportDao.GetRevenueByPaymentMethodByLocalDateRangeAsync(
            fromLocalDate,
            toLocalDate,
            localOffset,
            cancellationToken);

    public Task<IReadOnlyList<PaidBillHistoryItemDto>> GetPaidBillHistoryByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        string? paymentMethod,
        string? tableName,
        string? area,
        string? keyword,
        CancellationToken cancellationToken = default) =>
        _reportDao.GetPaidBillHistoryByLocalDateRangeAsync(
            fromLocalDate,
            toLocalDate,
            localOffset,
            paymentMethod,
            tableName,
            area,
            keyword,
            cancellationToken);
}
