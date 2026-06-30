using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Services.Reports;

public interface IPaidBillHistoryReportService
{
    Task<IReadOnlyList<PaidBillHistoryItemDto>> GetPaidBillHistoryAsync(
        PaidBillHistoryFilterDto filter,
        CancellationToken cancellationToken = default);
}
