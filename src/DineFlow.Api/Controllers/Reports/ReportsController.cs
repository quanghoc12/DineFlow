using DineFlow.BusinessObjects.Reports;
using DineFlow.Api.Controllers.Staff;
using DineFlow.Services.Reports;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Controllers.Reports;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : StaffControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IRevenueReportService _revenueReportService;
    private readonly ITopSellingItemReportService _topSellingItemReportService;
    private readonly IPaidBillHistoryReportService _paidBillHistoryReportService;
    private readonly IReportExportService _reportExportService;
    private readonly IDashboardAssistantService _dashboardAssistantService;

    public ReportsController(
        IDashboardService dashboardService,
        IRevenueReportService revenueReportService,
        ITopSellingItemReportService topSellingItemReportService,
        IPaidBillHistoryReportService paidBillHistoryReportService,
        IReportExportService reportExportService,
        IDashboardAssistantService dashboardAssistantService)
    {
        _dashboardService = dashboardService;
        _revenueReportService = revenueReportService;
        _topSellingItemReportService = topSellingItemReportService;
        _paidBillHistoryReportService = paidBillHistoryReportService;
        _reportExportService = reportExportService;
        _dashboardAssistantService = dashboardAssistantService;
    }

    [HttpGet("dashboard/today")]
    public async Task<ActionResult<DashboardDto>> GetToday(CancellationToken cancellationToken)
    {
        DashboardDto response = await _dashboardService.GetTodayDashboardAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetByDate(
        [FromQuery] DateTime? date,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        DashboardDto response;
        if (date.HasValue && toDate.HasValue)
        {
            response = await _dashboardService.GetDashboardRangeAsync(date.Value.Date, toDate.Value.Date, cancellationToken);
        }
        else if (date.HasValue)
        {
            response = await _dashboardService.GetDashboardByDateAsync(date.Value.Date, cancellationToken);
        }
        else
        {
            response = await _dashboardService.GetTodayDashboardAsync(cancellationToken);
        }

        return Ok(response);
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueSummaryDto>> GetRevenue(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken)
    {
        RevenueSummaryDto response = await _revenueReportService.GetRevenueSummaryAsync(
            fromDate.Date,
            toDate.Date,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("revenue/export/csv")]
    public async Task<IActionResult> ExportRevenueCsv(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken)
    {
        ExportedFileDto file = await _reportExportService.ExportRevenueSummaryCsvAsync(
            fromDate.Date,
            toDate.Date,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("revenue/export/excel")]
    public async Task<IActionResult> ExportRevenueExcel(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken)
    {
        ExportedFileDto file = await _reportExportService.ExportRevenueSummaryExcelAsync(
            fromDate.Date,
            toDate.Date,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("top-selling-items")]
    public async Task<ActionResult<IReadOnlyList<TopSellingItemDto>>> GetTopSellingItems(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TopSellingItemDto> response = await _topSellingItemReportService.GetTopSellingItemsAsync(
            fromDate.Date,
            toDate.Date,
            top,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("top-selling-items/export/csv")]
    public async Task<IActionResult> ExportTopSellingItemsCsv(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        ExportedFileDto file = await _reportExportService.ExportTopSellingItemsCsvAsync(
            fromDate.Date,
            toDate.Date,
            top,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("top-selling-items/export/excel")]
    public async Task<IActionResult> ExportTopSellingItemsExcel(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        ExportedFileDto file = await _reportExportService.ExportTopSellingItemsExcelAsync(
            fromDate.Date,
            toDate.Date,
            top,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("revenue/by-payment-method")]
    public async Task<ActionResult<IReadOnlyList<PaymentMethodRevenueDto>>> GetRevenueByPaymentMethod(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PaymentMethodRevenueDto> response = await _revenueReportService.GetRevenueByPaymentMethodAsync(
            fromDate.Date,
            toDate.Date,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("revenue/by-payment-method/export/csv")]
    public async Task<IActionResult> ExportRevenueByPaymentMethodCsv(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        ExportedFileDto file = await _reportExportService.ExportRevenueByPaymentMethodCsvAsync(
            fromDate.Date,
            toDate.Date,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("revenue/by-payment-method/export/excel")]
    public async Task<IActionResult> ExportRevenueByPaymentMethodExcel(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        ExportedFileDto file = await _reportExportService.ExportRevenueByPaymentMethodExcelAsync(
            fromDate.Date,
            toDate.Date,
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("paid-bill-history")]
    public async Task<ActionResult<IReadOnlyList<PaidBillHistoryItemDto>>> GetPaidBillHistory(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? paymentMethod,
        [FromQuery] string? tableName,
        [FromQuery] string? area,
        [FromQuery] string? keyword,
        CancellationToken cancellationToken = default)
    {
        PaidBillHistoryFilterDto filter = new()
        {
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            PaymentMethod = paymentMethod,
            TableName = tableName,
            Area = area,
            Keyword = keyword
        };

        IReadOnlyList<PaidBillHistoryItemDto> response = await _paidBillHistoryReportService.GetPaidBillHistoryAsync(
            filter,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("paid-bill-history/export/csv")]
    public async Task<IActionResult> ExportPaidBillHistoryCsv(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? paymentMethod,
        [FromQuery] string? tableName,
        [FromQuery] string? area,
        [FromQuery] string? keyword,
        CancellationToken cancellationToken = default)
    {
        ExportedFileDto file = await _reportExportService.ExportPaidBillHistoryCsvAsync(
            new PaidBillHistoryFilterDto
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                PaymentMethod = paymentMethod,
                TableName = tableName,
                Area = area,
                Keyword = keyword
            },
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("paid-bill-history/export/excel")]
    public async Task<IActionResult> ExportPaidBillHistoryExcel(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? paymentMethod,
        [FromQuery] string? tableName,
        [FromQuery] string? area,
        [FromQuery] string? keyword,
        CancellationToken cancellationToken = default)
    {
        ExportedFileDto file = await _reportExportService.ExportPaidBillHistoryExcelAsync(
            new PaidBillHistoryFilterDto
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                PaymentMethod = paymentMethod,
                TableName = tableName,
                Area = area,
                Keyword = keyword
            },
            cancellationToken);

        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("cancellations")]
    public async Task<ActionResult<CancellationSummaryDto>> GetCancellations(
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        DateTime localDate = date?.Date ?? DateTime.Today;
        CancellationSummaryDto response = await _dashboardService.GetCancellationSummaryAsync(localDate, cancellationToken);
        return Ok(response);
    }

    [HttpPost("assistant/chat")]
    public async Task<ActionResult<DashboardAssistantChatResponseDto>> ChatWithAssistant(
        [FromBody] DashboardAssistantChatRequestDto request,
        CancellationToken cancellationToken)
    {
        DashboardAssistantChatResponseDto response = await _dashboardAssistantService.ChatAsync(
            request,
            CurrentUserId,
            CurrentUserRole,
            cancellationToken);

        return Ok(response);
    }
}
