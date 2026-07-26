using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Services.Reports;

public interface IDashboardService
{
    Task<DashboardDto> GetTodayDashboardAsync(CancellationToken cancellationToken = default);
    Task<DashboardDto> GetDashboardByDateAsync(DateTime localDate, CancellationToken cancellationToken = default);
    Task<DashboardDto> GetDashboardRangeAsync(DateTime fromLocalDate, DateTime toLocalDate, CancellationToken cancellationToken = default);
    Task<CancellationSummaryDto> GetCancellationSummaryAsync(DateTime localDate, CancellationToken cancellationToken = default);
}
