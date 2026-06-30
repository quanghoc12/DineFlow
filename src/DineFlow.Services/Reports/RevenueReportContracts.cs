using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Services.Reports;

public interface IRevenueReportService
{
    Task<RevenueSummaryDto> GetRevenueSummaryAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentMethodRevenueDto>> GetRevenueByPaymentMethodAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default);
}

public interface IReportExportService
{
    Task<ExportedFileDto> ExportRevenueSummaryCsvAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default);

    Task<ExportedFileDto> ExportRevenueSummaryExcelAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default);

    Task<ExportedFileDto> ExportTopSellingItemsCsvAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        int topCount,
        CancellationToken cancellationToken = default);

    Task<ExportedFileDto> ExportTopSellingItemsExcelAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        int topCount,
        CancellationToken cancellationToken = default);

    Task<ExportedFileDto> ExportRevenueByPaymentMethodCsvAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default);

    Task<ExportedFileDto> ExportRevenueByPaymentMethodExcelAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default);

    Task<ExportedFileDto> ExportPaidBillHistoryCsvAsync(
        PaidBillHistoryFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<ExportedFileDto> ExportPaidBillHistoryExcelAsync(
        PaidBillHistoryFilterDto filter,
        CancellationToken cancellationToken = default);
}
