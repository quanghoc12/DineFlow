using System.Text;
using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Services.Reports;

public sealed class ReportExportService : IReportExportService
{
    private readonly IRevenueReportService _revenueReportService;
    private readonly ITopSellingItemReportService _topSellingItemReportService;
    private readonly IPaidBillHistoryReportService _paidBillHistoryReportService;

    public ReportExportService(
        IRevenueReportService revenueReportService,
        ITopSellingItemReportService topSellingItemReportService,
        IPaidBillHistoryReportService paidBillHistoryReportService)
    {
        _revenueReportService = revenueReportService;
        _topSellingItemReportService = topSellingItemReportService;
        _paidBillHistoryReportService = paidBillHistoryReportService;
    }

    public async Task<ExportedFileDto> ExportRevenueSummaryCsvAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default)
    {
        RevenueSummaryDto summary = await _revenueReportService.GetRevenueSummaryAsync(
            fromLocalDate,
            toLocalDate,
            cancellationToken);

        StringBuilder builder = new();
        builder.AppendLine("Date,Revenue,PaidBillCount");
        foreach (RevenueByDayDto day in summary.RevenueByDays)
        {
            builder.AppendLine($"{day.Date:yyyy-MM-dd},{day.Revenue:0.##},{day.PaidBillCount}");
        }

        builder.AppendLine();
        builder.AppendLine("FromDate,ToDate,TotalRevenue,PaidBillCount,AverageBillValue");
        builder.AppendLine($"{summary.FromDate:yyyy-MM-dd},{summary.ToDate:yyyy-MM-dd},{summary.TotalRevenue:0.##},{summary.PaidBillCount},{summary.AverageBillValue:0.##}");

        return new ExportedFileDto
        {
            FileName = $"revenue-summary-{summary.FromDate:yyyyMMdd}-{summary.ToDate:yyyyMMdd}.csv",
            ContentType = "text/csv; charset=utf-8",
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray()
        };
    }

    public async Task<ExportedFileDto> ExportRevenueSummaryExcelAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default)
    {
        RevenueSummaryDto summary = await _revenueReportService.GetRevenueSummaryAsync(
            fromLocalDate,
            toLocalDate,
            cancellationToken);

        StringBuilder html = new();
        html.AppendLine("<html><head><meta charset=\"utf-8\" /></head><body>");
        html.AppendLine("<h2>Revenue Summary</h2>");
        html.AppendLine($"<p>From: {summary.FromDate:yyyy-MM-dd} | To: {summary.ToDate:yyyy-MM-dd}</p>");
        html.AppendLine("<table border=\"1\"><tr><th>Date</th><th>Revenue</th><th>Paid Bill Count</th></tr>");
        foreach (RevenueByDayDto day in summary.RevenueByDays)
        {
            html.AppendLine($"<tr><td>{day.Date:yyyy-MM-dd}</td><td>{day.Revenue:0.##}</td><td>{day.PaidBillCount}</td></tr>");
        }
        html.AppendLine("</table>");
        html.AppendLine("<br />");
        html.AppendLine("<table border=\"1\"><tr><th>Total Revenue</th><th>Paid Bill Count</th><th>Average Bill Value</th></tr>");
        html.AppendLine($"<tr><td>{summary.TotalRevenue:0.##}</td><td>{summary.PaidBillCount}</td><td>{summary.AverageBillValue:0.##}</td></tr>");
        html.AppendLine("</table>");
        html.AppendLine("</body></html>");

        return new ExportedFileDto
        {
            FileName = $"revenue-summary-{summary.FromDate:yyyyMMdd}-{summary.ToDate:yyyyMMdd}.xls",
            ContentType = "application/vnd.ms-excel",
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(html.ToString())).ToArray()
        };
    }

    public async Task<ExportedFileDto> ExportTopSellingItemsCsvAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        int topCount,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TopSellingItemDto> items = await _topSellingItemReportService.GetTopSellingItemsAsync(
            fromLocalDate,
            toLocalDate,
            topCount,
            cancellationToken);

        StringBuilder builder = new();
        builder.AppendLine("Rank,MenuItemId,ItemName,TotalQuantity,TotalRevenue");
        for (int index = 0; index < items.Count; index++)
        {
            TopSellingItemDto item = items[index];
            builder.AppendLine($"{index + 1},{item.MenuItemId},\"{item.ItemName.Replace("\"", "\"\"")}\",{item.TotalQuantity},{item.TotalRevenue:0.##}");
        }

        return new ExportedFileDto
        {
            FileName = $"top-selling-{fromLocalDate:yyyyMMdd}-{toLocalDate:yyyyMMdd}-top{topCount}.csv",
            ContentType = "text/csv; charset=utf-8",
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray()
        };
    }

    public async Task<ExportedFileDto> ExportTopSellingItemsExcelAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        int topCount,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TopSellingItemDto> items = await _topSellingItemReportService.GetTopSellingItemsAsync(
            fromLocalDate,
            toLocalDate,
            topCount,
            cancellationToken);

        StringBuilder html = new();
        html.AppendLine("<html><head><meta charset=\"utf-8\" /></head><body>");
        html.AppendLine("<h2>Top Selling Items</h2>");
        html.AppendLine($"<p>From: {fromLocalDate:yyyy-MM-dd} | To: {toLocalDate:yyyy-MM-dd} | Top: {topCount}</p>");
        html.AppendLine("<table border=\"1\"><tr><th>Rank</th><th>MenuItemId</th><th>ItemName</th><th>TotalQuantity</th><th>TotalRevenue</th></tr>");
        for (int index = 0; index < items.Count; index++)
        {
            TopSellingItemDto item = items[index];
            html.AppendLine($"<tr><td>{index + 1}</td><td>{item.MenuItemId}</td><td>{System.Net.WebUtility.HtmlEncode(item.ItemName)}</td><td>{item.TotalQuantity}</td><td>{item.TotalRevenue:0.##}</td></tr>");
        }
        html.AppendLine("</table>");
        html.AppendLine("</body></html>");

        return new ExportedFileDto
        {
            FileName = $"top-selling-{fromLocalDate:yyyyMMdd}-{toLocalDate:yyyyMMdd}-top{topCount}.xls",
            ContentType = "application/vnd.ms-excel",
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(html.ToString())).ToArray()
        };
    }

    public async Task<ExportedFileDto> ExportRevenueByPaymentMethodCsvAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PaymentMethodRevenueDto> items = await _revenueReportService.GetRevenueByPaymentMethodAsync(
            fromLocalDate,
            toLocalDate,
            cancellationToken);

        StringBuilder builder = new();
        builder.AppendLine("PaymentMethod,PaymentCount,TotalAmount");
        foreach (PaymentMethodRevenueDto item in items)
        {
            builder.AppendLine($"{item.PaymentMethod},{item.PaymentCount},{item.TotalAmount:0.##}");
        }

        return new ExportedFileDto
        {
            FileName = $"payment-method-revenue-{fromLocalDate:yyyyMMdd}-{toLocalDate:yyyyMMdd}.csv",
            ContentType = "text/csv; charset=utf-8",
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray()
        };
    }

    public async Task<ExportedFileDto> ExportRevenueByPaymentMethodExcelAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PaymentMethodRevenueDto> items = await _revenueReportService.GetRevenueByPaymentMethodAsync(
            fromLocalDate,
            toLocalDate,
            cancellationToken);

        StringBuilder html = new();
        html.AppendLine("<html><head><meta charset=\"utf-8\" /></head><body>");
        html.AppendLine("<h2>Revenue By Payment Method</h2>");
        html.AppendLine($"<p>From: {fromLocalDate:yyyy-MM-dd} | To: {toLocalDate:yyyy-MM-dd}</p>");
        html.AppendLine("<table border=\"1\"><tr><th>PaymentMethod</th><th>PaymentCount</th><th>TotalAmount</th></tr>");
        foreach (PaymentMethodRevenueDto item in items)
        {
            html.AppendLine($"<tr><td>{System.Net.WebUtility.HtmlEncode(item.PaymentMethod)}</td><td>{item.PaymentCount}</td><td>{item.TotalAmount:0.##}</td></tr>");
        }
        html.AppendLine("</table>");
        html.AppendLine("</body></html>");

        return new ExportedFileDto
        {
            FileName = $"payment-method-revenue-{fromLocalDate:yyyyMMdd}-{toLocalDate:yyyyMMdd}.xls",
            ContentType = "application/vnd.ms-excel",
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(html.ToString())).ToArray()
        };
    }

    public async Task<ExportedFileDto> ExportPaidBillHistoryCsvAsync(
        PaidBillHistoryFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PaidBillHistoryItemDto> items = await _paidBillHistoryReportService.GetPaidBillHistoryAsync(
            filter,
            cancellationToken);

        StringBuilder builder = new();
        builder.AppendLine("PaymentId,BillId,BillCode,BillName,TableName,Area,PaymentMethod,PaymentAmount,BillFinalAmount,PaidAt,ConfirmedByUserId,ConfirmedByName,UpdatedAt,UpdatedByUserId,UpdatedByName,ChangeReason,IsCorrected");
        foreach (PaidBillHistoryItemDto item in items)
        {
            builder.AppendLine(string.Join(",",
                item.PaymentId,
                item.BillId,
                EscapeCsv(item.BillCode),
                EscapeCsv(item.BillName),
                EscapeCsv(item.TableName),
                EscapeCsv(item.Area),
                EscapeCsv(item.PaymentMethod),
                item.PaymentAmount.ToString("0.##"),
                item.BillFinalAmount.ToString("0.##"),
                item.PaidAt.ToString("yyyy-MM-dd HH:mm:ss"),
                item.ConfirmedByUserId,
                EscapeCsv(item.ConfirmedByName),
                item.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                item.UpdatedByUserId?.ToString() ?? string.Empty,
                EscapeCsv(item.UpdatedByName),
                EscapeCsv(item.ChangeReason),
                item.IsCorrected));
        }

        return new ExportedFileDto
        {
            FileName = $"paid-bill-history-{filter.FromDate:yyyyMMdd}-{filter.ToDate:yyyyMMdd}.csv",
            ContentType = "text/csv; charset=utf-8",
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray()
        };
    }

    public async Task<ExportedFileDto> ExportPaidBillHistoryExcelAsync(
        PaidBillHistoryFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PaidBillHistoryItemDto> items = await _paidBillHistoryReportService.GetPaidBillHistoryAsync(
            filter,
            cancellationToken);

        StringBuilder html = new();
        html.AppendLine("<html><head><meta charset=\"utf-8\" /></head><body>");
        html.AppendLine("<h2>Paid Bill History</h2>");
        html.AppendLine($"<p>From: {filter.FromDate:yyyy-MM-dd} | To: {filter.ToDate:yyyy-MM-dd}</p>");
        html.AppendLine("<table border=\"1\"><tr><th>PaymentId</th><th>BillId</th><th>BillCode</th><th>BillName</th><th>TableName</th><th>Area</th><th>PaymentMethod</th><th>PaymentAmount</th><th>BillFinalAmount</th><th>PaidAt</th><th>ConfirmedBy</th><th>UpdatedAt</th><th>UpdatedBy</th><th>ChangeReason</th><th>IsCorrected</th></tr>");
        foreach (PaidBillHistoryItemDto item in items)
        {
            html.AppendLine(
                $"<tr><td>{item.PaymentId}</td><td>{item.BillId}</td><td>{System.Net.WebUtility.HtmlEncode(item.BillCode)}</td><td>{System.Net.WebUtility.HtmlEncode(item.BillName)}</td><td>{System.Net.WebUtility.HtmlEncode(item.TableName)}</td><td>{System.Net.WebUtility.HtmlEncode(item.Area)}</td><td>{System.Net.WebUtility.HtmlEncode(item.PaymentMethod)}</td><td>{item.PaymentAmount:0.##}</td><td>{item.BillFinalAmount:0.##}</td><td>{item.PaidAt:yyyy-MM-dd HH:mm:ss}</td><td>{System.Net.WebUtility.HtmlEncode(item.ConfirmedByName)}</td><td>{(item.UpdatedAt.HasValue ? item.UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty)}</td><td>{System.Net.WebUtility.HtmlEncode(item.UpdatedByName)}</td><td>{System.Net.WebUtility.HtmlEncode(item.ChangeReason)}</td><td>{(item.IsCorrected ? "Yes" : "No")}</td></tr>");
        }
        html.AppendLine("</table>");
        html.AppendLine("</body></html>");

        return new ExportedFileDto
        {
            FileName = $"paid-bill-history-{filter.FromDate:yyyyMMdd}-{filter.ToDate:yyyyMMdd}.xls",
            ContentType = "application/vnd.ms-excel",
            Content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(html.ToString())).ToArray()
        };
    }

    private static string EscapeCsv(string? value)
    {
        string safeValue = value ?? string.Empty;
        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }
}
