namespace DineFlow.BusinessObjects.Reports;

public sealed class DashboardDto
{
    public DateTime Date { get; set; }
    public decimal RevenueToday { get; set; }
    public int PaidBillCountToday { get; set; }
    public decimal AverageBillValue { get; set; }
    public int OrderCountToday { get; set; }
    public int ServingTableCount { get; set; }
    public int WaitingPaymentTableCount { get; set; }
    public int PrintFailedOrderCount { get; set; }
    public List<TopSellingItemDto> TopSellingItems { get; set; } = [];
    public List<PaymentMethodRevenueDto> RevenueByPaymentMethods { get; set; } = [];
}

public sealed class TopSellingItemDto
{
    public int MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}

public sealed class RevenueSummaryDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PaidBillCount { get; set; }
    public decimal AverageBillValue { get; set; }
    public List<RevenueByDayDto> RevenueByDays { get; set; } = [];
}

public sealed class RevenueByDayDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int PaidBillCount { get; set; }
}

public sealed class PaymentMethodRevenueDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public int PaymentCount { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class PaidBillHistoryFilterDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TableName { get; set; }
    public string? Area { get; set; }
    public string? Keyword { get; set; }
}

public sealed class PaidBillHistoryItemDto
{
    public int PaymentId { get; set; }
    public int BillId { get; set; }
    public string BillCode { get; set; } = string.Empty;
    public string BillName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal PaymentAmount { get; set; }
    public decimal BillFinalAmount { get; set; }
    public DateTime PaidAt { get; set; }
    public int ConfirmedByUserId { get; set; }
    public string ConfirmedByName { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
    public string UpdatedByName { get; set; } = string.Empty;
    public string ChangeReason { get; set; } = string.Empty;
    public bool IsCorrected { get; set; }
}

public sealed class ExportedFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = [];
}

public sealed class CancelledBillDto
{
    public int BillId { get; set; }
    public string BillCode { get; set; } = string.Empty;
    public string BillName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
    public DateTime CancelledAt { get; set; }
    public string CancelledByName { get; set; } = string.Empty;
    public string CancelReason { get; set; } = string.Empty;
}

public sealed class CancelledItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
    public string CancelledByName { get; set; } = string.Empty;
    public string CancelReason { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // "Order" or "Bill"
}

public sealed class CancellationSummaryDto
{
    public int CancelledBillCount { get; set; }
    public int CancelledItemCount { get; set; }
    public List<CancelledBillDto> CancelledBills { get; set; } = [];
    public List<CancelledItemDto> CancelledItems { get; set; } = [];
}

public sealed class DashboardAssistantChatRequestDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<DashboardAssistantMessageDto> Messages { get; set; } = [];
    public DashboardAssistantContextDto Context { get; set; } = new();
}

public sealed class DashboardAssistantChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public List<string> SuggestedQuestions { get; set; } = [];
    public string UsedDataRange { get; set; } = string.Empty;
    public bool UsedCachedData { get; set; }
    public List<string> Warnings { get; set; } = [];
}

public sealed class DashboardAssistantMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public sealed class DashboardAssistantContextDto
{
    public string CurrentTab { get; set; } = "Overview";
    public string ChartMode { get; set; } = "Last7Days";
    public string TopSellingPeriod { get; set; } = "Today";
    public string PaymentPeriod { get; set; } = "Today";
}

public sealed class AssistantBusinessContextTextDto
{
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
