using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Services.Reports;

public interface ITopSellingItemReportService
{
    Task<IReadOnlyList<TopSellingItemDto>> GetTopSellingItemsAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        int topCount,
        CancellationToken cancellationToken = default);
}
