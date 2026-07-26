using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Services.Reports;

internal interface IDashboardAssistantDataProvider
{
    Task<DashboardAssistantDataContext> GetDataAsync(
        DashboardAssistantPlan plan,
        DashboardAssistantSessionState sessionState,
        CancellationToken cancellationToken = default);
}

internal sealed class DashboardAssistantDataProvider : IDashboardAssistantDataProvider
{
    private const int TopItemCount = 10;
    private const int PaidHistoryCount = 15;

    private readonly IDashboardService _dashboardService;
    private readonly IRevenueReportService _revenueReportService;
    private readonly ITopSellingItemReportService _topSellingItemReportService;
    private readonly IPaidBillHistoryReportService _paidBillHistoryReportService;

    public DashboardAssistantDataProvider(
        IDashboardService dashboardService,
        IRevenueReportService revenueReportService,
        ITopSellingItemReportService topSellingItemReportService,
        IPaidBillHistoryReportService paidBillHistoryReportService)
    {
        _dashboardService = dashboardService;
        _revenueReportService = revenueReportService;
        _topSellingItemReportService = topSellingItemReportService;
        _paidBillHistoryReportService = paidBillHistoryReportService;
    }

    public async Task<DashboardAssistantDataContext> GetDataAsync(
        DashboardAssistantPlan plan,
        DashboardAssistantSessionState sessionState,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DashboardAssistantDataContext context = new()
        {
            Intent = plan.Intent.ToString(),
            FromDate = plan.FromDate,
            ToDate = plan.ToDate,
            RangeLabel = plan.RangeLabel,
            ReusedPreviousRange = plan.ReusedPreviousRange
        };

        switch (plan.Intent)
        {
            case DashboardAssistantIntent.TopSelling:
                context.Snapshots.Add(await GetSnapshotAsync(
                    sessionState,
                    BuildKey("TopSelling", plan, $"Top{TopItemCount}"),
                    isRealtime: IsToday(plan),
                    now,
                    () => _topSellingItemReportService.GetTopSellingItemsAsync(plan.FromDate, plan.ToDate, TopItemCount, cancellationToken)));
                break;

            case DashboardAssistantIntent.Payment:
                context.Snapshots.Add(await GetSnapshotAsync(
                    sessionState,
                    BuildKey("PaymentMethods", plan),
                    isRealtime: IsToday(plan),
                    now,
                    () => _revenueReportService.GetRevenueByPaymentMethodAsync(plan.FromDate, plan.ToDate, cancellationToken)));
                context.Snapshots.Add(await GetSnapshotAsync(
                    sessionState,
                    BuildKey("PaidHistory", plan, $"Top{PaidHistoryCount}"),
                    isRealtime: IsToday(plan),
                    now,
                    async () => (await _paidBillHistoryReportService.GetPaidBillHistoryAsync(
                        new PaidBillHistoryFilterDto { FromDate = plan.FromDate, ToDate = plan.ToDate },
                        cancellationToken)).Take(PaidHistoryCount).ToList()));
                break;

            case DashboardAssistantIntent.Cancellation:
                context.Snapshots.Add(await GetSnapshotAsync(
                    sessionState,
                    BuildKey("Cancellations", plan),
                    isRealtime: IsToday(plan),
                    now,
                    () => GetCancellationRangeAsync(plan, cancellationToken)));
                break;

            case DashboardAssistantIntent.Operations:
                context.Snapshots.Add(await GetSnapshotAsync(
                    sessionState,
                    BuildKey("Dashboard", plan),
                    isRealtime: IsToday(plan),
                    now,
                    () => GetDashboardAsync(plan, cancellationToken)));
                break;

            case DashboardAssistantIntent.Revenue:
                context.Snapshots.Add(await GetSnapshotAsync(
                    sessionState,
                    BuildKey("RevenueSummaryComplete", plan),
                    isRealtime: IsToday(plan),
                    now,
                    () => GetRevenueSummarySnapshotAsync(plan, cancellationToken)));
                break;

            default:
                context.Snapshots.Add(await GetSnapshotAsync(
                    sessionState,
                    BuildKey("Dashboard", plan),
                    isRealtime: IsToday(plan),
                    now,
                    () => GetDashboardAsync(plan, cancellationToken)));
                context.Snapshots.Add(await GetSnapshotAsync(
                    sessionState,
                    BuildKey("RevenueSummaryComplete", plan),
                    isRealtime: IsToday(plan),
                    now,
                    () => GetRevenueSummarySnapshotAsync(plan, cancellationToken)));
                break;
        }

        context.UsedCachedData = context.Snapshots.Count > 0 && context.Snapshots.All(x => x.FromCache);
        sessionState.LastFromDate = plan.FromDate;
        sessionState.LastToDate = plan.ToDate;
        return context;
    }

    private async Task<DashboardAssistantSnapshot> GetSnapshotAsync<T>(
        DashboardAssistantSessionState sessionState,
        string key,
        bool isRealtime,
        DateTimeOffset now,
        Func<Task<T>> fetchAsync)
    {
        if (sessionState.TryGetSnapshot(key, now, out object? cached) && cached is T typed)
        {
            return new DashboardAssistantSnapshot
            {
                Key = key,
                Data = typed!,
                FromCache = true
            };
        }

        T data = await fetchAsync();
        sessionState.SetSnapshot(key, data!, isRealtime, now);
        return new DashboardAssistantSnapshot
        {
            Key = key,
            Data = data!,
            FromCache = false
        };
    }

    private Task<DashboardDto> GetDashboardAsync(
        DashboardAssistantPlan plan,
        CancellationToken cancellationToken)
    {
        return plan.FromDate.Date == plan.ToDate.Date
            ? _dashboardService.GetDashboardByDateAsync(plan.FromDate, cancellationToken)
            : _dashboardService.GetDashboardRangeAsync(plan.FromDate, plan.ToDate, cancellationToken);
    }

    private async Task<CancellationRangeSnapshot> GetCancellationRangeAsync(
        DashboardAssistantPlan plan,
        CancellationToken cancellationToken)
    {
        int dayCount = Math.Min((plan.ToDate.Date - plan.FromDate.Date).Days + 1, 30);
        List<CancellationSummaryDto> summaries = [];

        for (int i = 0; i < dayCount; i++)
        {
            DateTime date = plan.FromDate.Date.AddDays(i);
            summaries.Add(await _dashboardService.GetCancellationSummaryAsync(date, cancellationToken));
        }

        return new CancellationRangeSnapshot
        {
            FromDate = plan.FromDate,
            ToDate = plan.FromDate.AddDays(dayCount - 1),
            CancelledBillCount = summaries.Sum(x => x.CancelledBillCount),
            CancelledItemCount = summaries.Sum(x => x.CancelledItemCount),
            RecentCancelledBills = summaries.SelectMany(x => x.CancelledBills)
                .OrderByDescending(x => x.CancelledAt)
                .Take(15)
                .ToList(),
            RecentCancelledItems = summaries.SelectMany(x => x.CancelledItems)
                .OrderByDescending(x => x.CancelledAt)
                .Take(15)
                .ToList()
        };
    }

    private async Task<RevenueSummarySnapshot> GetRevenueSummarySnapshotAsync(
        DashboardAssistantPlan plan,
        CancellationToken cancellationToken)
    {
        RevenueSummaryDto summary = await _revenueReportService.GetRevenueSummaryAsync(
            plan.FromDate,
            plan.ToDate,
            cancellationToken);

        Dictionary<DateTime, RevenueByDayDto> revenueByDate = summary.RevenueByDays
            .GroupBy(x => x.Date.Date)
            .ToDictionary(
                group => group.Key,
                group => new RevenueByDayDto
                {
                    Date = group.Key,
                    Revenue = group.Sum(x => x.Revenue),
                    PaidBillCount = group.Sum(x => x.PaidBillCount)
                });

        int dayCount = Math.Min((plan.ToDate.Date - plan.FromDate.Date).Days + 1, 90);
        List<RevenueDaySnapshot> dailyRevenue = [];

        for (int i = 0; i < dayCount; i++)
        {
            DateTime date = plan.FromDate.Date.AddDays(i);
            revenueByDate.TryGetValue(date, out RevenueByDayDto? day);
            dailyRevenue.Add(new RevenueDaySnapshot
            {
                Date = date,
                Revenue = day?.Revenue ?? 0m,
                PaidBillCount = day?.PaidBillCount ?? 0,
                HasPaidBills = (day?.PaidBillCount ?? 0) > 0
            });
        }

        RevenueDaySnapshot? today = dailyRevenue.LastOrDefault();
        List<RevenueDaySnapshot> previousDays = dailyRevenue.Count > 1
            ? dailyRevenue.Take(dailyRevenue.Count - 1).ToList()
            : [];

        decimal previousDaysAverageRevenue = previousDays.Count == 0
            ? 0m
            : previousDays.Average(x => x.Revenue);

        return new RevenueSummarySnapshot
        {
            FromDate = plan.FromDate,
            ToDate = plan.ToDate,
            TotalRevenue = summary.TotalRevenue,
            PaidBillCount = summary.PaidBillCount,
            AverageBillValue = summary.AverageBillValue,
            DailyRevenue = dailyRevenue,
            LastDayRevenue = today?.Revenue ?? 0m,
            PreviousDaysAverageRevenue = previousDaysAverageRevenue,
            DifferenceFromPreviousDaysAverage = today is null ? 0m : today.Revenue - previousDaysAverageRevenue,
            Note = "DailyRevenue contains every date in the requested range. Revenue 0 means the date was checked and no paid bills were found."
        };
    }

    private static string BuildKey(string dataKind, DashboardAssistantPlan plan, string? filter = null)
    {
        string key = $"{dataKind}:{plan.FromDate:yyyy-MM-dd}:{plan.ToDate:yyyy-MM-dd}";
        return string.IsNullOrWhiteSpace(filter) ? key : $"{key}:{filter}";
    }

    private static bool IsToday(DashboardAssistantPlan plan) => plan.ToDate.Date >= DateTime.Today;
}

internal sealed class DashboardAssistantDataContext
{
    public string Intent { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string RangeLabel { get; set; } = string.Empty;
    public bool ReusedPreviousRange { get; set; }
    public bool UsedCachedData { get; set; }
    public List<DashboardAssistantSnapshot> Snapshots { get; set; } = [];
}

internal sealed class DashboardAssistantSnapshot
{
    public string Key { get; set; } = string.Empty;
    public bool FromCache { get; set; }
    public object Data { get; set; } = new();
}

internal sealed class CancellationRangeSnapshot
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int CancelledBillCount { get; set; }
    public int CancelledItemCount { get; set; }
    public List<CancelledBillDto> RecentCancelledBills { get; set; } = [];
    public List<CancelledItemDto> RecentCancelledItems { get; set; } = [];
}

internal sealed class RevenueSummarySnapshot
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PaidBillCount { get; set; }
    public decimal AverageBillValue { get; set; }
    public decimal LastDayRevenue { get; set; }
    public decimal PreviousDaysAverageRevenue { get; set; }
    public decimal DifferenceFromPreviousDaysAverage { get; set; }
    public string Note { get; set; } = string.Empty;
    public List<RevenueDaySnapshot> DailyRevenue { get; set; } = [];
}

internal sealed class RevenueDaySnapshot
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int PaidBillCount { get; set; }
    public bool HasPaidBills { get; set; }
}
