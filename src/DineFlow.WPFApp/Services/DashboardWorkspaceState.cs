namespace DineFlow.WPFApp.Services;

public static class DashboardWorkspaceState
{
    public static event Action<DashboardWorkspaceNavigationRequest>? NavigationRequested;

    public static void OpenOverview() => Raise(DashboardTabs.Overview);
    public static void OpenRevenue() => Raise(DashboardTabs.Revenue);
    public static void OpenTopSelling() => Raise(DashboardTabs.TopSelling);
    public static void OpenPaymentMethodRevenue() => Raise(DashboardTabs.PaymentMethodRevenue);
    public static void OpenPaidBillHistory() => Raise(DashboardTabs.PaidBillHistory);
    public static void OpenPaymentCorrection(int? billId = null) => Raise(DashboardTabs.PaymentCorrection, billId);

    private static void Raise(string tabKey, int? billId = null)
    {
        NavigationRequested?.Invoke(new DashboardWorkspaceNavigationRequest(tabKey, billId));
    }
}

public static class DashboardTabs
{
    public const string Overview = "Overview";
    public const string Revenue = "Revenue";
    public const string TopSelling = "TopSelling";
    public const string PaymentMethodRevenue = "PaymentMethodRevenue";
    public const string PaidBillHistory = "PaidBillHistory";
    public const string PaymentCorrection = "PaymentCorrection";
}

public sealed record DashboardWorkspaceNavigationRequest(string TabKey, int? BillId);
