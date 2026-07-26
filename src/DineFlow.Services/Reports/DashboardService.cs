using DineFlow.BusinessObjects.Reports;
using DineFlow.Repositories.Reports;

namespace DineFlow.Services.Reports;

public sealed class DashboardService : IDashboardService
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    private const int DashboardTopItemCount = 5;

    private readonly IReportRepository _reportRepository;

    public DashboardService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public Task<DashboardDto> GetTodayDashboardAsync(CancellationToken cancellationToken = default)
    {
        DateTime localNow = DateTimeOffset.UtcNow.ToOffset(VietnamOffset).DateTime;
        return GetDashboardByDateAsync(localNow, cancellationToken);
    }

    public Task<DashboardDto> GetDashboardByDateAsync(DateTime localDate, CancellationToken cancellationToken = default) =>
        _reportRepository.GetDashboardByLocalDateAsync(localDate, VietnamOffset, DashboardTopItemCount, cancellationToken);

    public Task<DashboardDto> GetDashboardRangeAsync(DateTime fromLocalDate, DateTime toLocalDate, CancellationToken cancellationToken = default) =>
        _reportRepository.GetDashboardByLocalDateRangeAsync(fromLocalDate, toLocalDate, VietnamOffset, DashboardTopItemCount, cancellationToken);

    public Task<CancellationSummaryDto> GetCancellationSummaryAsync(DateTime localDate, CancellationToken cancellationToken = default) =>
        _reportRepository.GetCancellationSummaryByLocalDateAsync(localDate, VietnamOffset, cancellationToken);
}
