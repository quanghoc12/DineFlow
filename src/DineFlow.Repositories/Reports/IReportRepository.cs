using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Repositories.Reports;

public interface IReportRepository
{
    Task<DashboardDto> GetDashboardByLocalDateAsync(
        DateTime localDate,
        TimeSpan localOffset,
        int topItemCount,
        CancellationToken cancellationToken = default);

    Task<RevenueSummaryDto> GetRevenueSummaryByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopSellingItemDto>> GetTopSellingItemsByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        int topCount,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentMethodRevenueDto>> GetRevenueByPaymentMethodByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaidBillHistoryItemDto>> GetPaidBillHistoryByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        string? paymentMethod,
        string? tableName,
        string? area,
        string? keyword,
        CancellationToken cancellationToken = default);
}
