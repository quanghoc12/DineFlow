namespace DineFlow.BusinessObjects.Reports;

public class DashboardDto
{
    public decimal TodayRevenue { get; set; }
    public int PaidBillCount { get; set; }
    public int TodayOrderCount { get; set; }
    public decimal AverageBillValue { get; set; }
    public int OccupiedTableCount { get; set; }
    public int WaitingPaymentTableCount { get; set; }
    public int PrintFailedOrderCount { get; set; }
}
