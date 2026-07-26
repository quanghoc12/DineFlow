namespace DineFlow.WPFApp.Services;

public static class DashboardWorkspaceState
{
    public static event Action<DashboardWorkspaceNavigationRequest>? NavigationRequested;

    public static void OpenOverview() => Raise(DashboardTabs.Overview);
    public static void OpenRevenue() => Raise(DashboardTabs.Revenue);
    public static void OpenCancellation() => Raise(DashboardTabs.Cancellation);

    private static void Raise(string tabKey, int? billId = null)
    {
        NavigationRequested?.Invoke(new DashboardWorkspaceNavigationRequest(tabKey, billId));
    }
}

public static class DashboardTabs
{
    public const string Overview = "Overview";
    public const string Revenue = "Revenue";
    public const string Cancellation = "Cancellation";
}

public sealed record DashboardWorkspaceNavigationRequest(string TabKey, int? BillId);
