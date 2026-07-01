using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Services.Reports;

public interface IDashboardService
{
    Task<DashboardDto> GetTodayDashboardAsync(CancellationToken cancellationToken = default);
    Task<DashboardDto> GetDashboardByDateAsync(DateTime localDate, CancellationToken cancellationToken = default);
}
