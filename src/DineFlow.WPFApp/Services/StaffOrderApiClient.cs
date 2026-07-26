using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Reports;
using DineFlow.Services.Bills;
using DineFlow.Services.Menu;
using DineFlow.Services.Orders;
using DineFlow.Services.Requests;

namespace DineFlow.WPFApp.Services;

public sealed class StaffOrderApiClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public StaffOrderApiClient(string? baseAddress = null)
    {
        baseAddress ??= AppClientSettings.ResolveApiBaseUrl();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(45)
        };

        _httpClient.DefaultRequestHeaders.Add("X-User-Id", ApiClientSession.CurrentUserId.ToString());
        _httpClient.DefaultRequestHeaders.Add("X-User-Role", ApiClientSession.CurrentUserRole);
    }

    public async Task<IReadOnlyList<DiningTableDto>> GetTablesAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<DiningTableDto>>(
            "api/staff/tables?activeOnly=true",
            cancellationToken) ?? [];
    }

    public async Task<DashboardDto> GetTodayDashboardAsync(CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<DashboardDto>(
            "api/reports/dashboard/today",
            cancellationToken) ?? new DashboardDto();
    }

    public async Task<DashboardDto> GetDashboardByDateAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<DashboardDto>(
            $"api/reports/dashboard?date={date:yyyy-MM-dd}",
            cancellationToken) ?? new DashboardDto();
    }

    public async Task<DashboardDto> GetDashboardByRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<DashboardDto>(
            $"api/reports/dashboard?date={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}",
            cancellationToken) ?? new DashboardDto();
    }

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<RevenueSummaryDto>(
            $"api/reports/revenue?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}",
            cancellationToken) ?? new RevenueSummaryDto();
    }

    public async Task<byte[]> ExportRevenueSummaryCsvAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await GetBytesAsync(
            $"api/reports/revenue/export/csv?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}",
            cancellationToken);
    }

    public async Task<byte[]> ExportRevenueSummaryExcelAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await GetBytesAsync(
            $"api/reports/revenue/export/excel?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}",
            cancellationToken);
    }

    public async Task<IReadOnlyList<TopSellingItemDto>> GetTopSellingItemsAsync(
        DateTime fromDate,
        DateTime toDate,
        int topCount,
        CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<IReadOnlyList<TopSellingItemDto>>(
            $"api/reports/top-selling-items?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}&top={topCount}",
            cancellationToken) ?? [];
    }

    public async Task<byte[]> ExportTopSellingItemsCsvAsync(
        DateTime fromDate,
        DateTime toDate,
        int topCount,
        CancellationToken cancellationToken = default)
    {
        return await GetBytesAsync(
            $"api/reports/top-selling-items/export/csv?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}&top={topCount}",
            cancellationToken);
    }

    public async Task<byte[]> ExportTopSellingItemsExcelAsync(
        DateTime fromDate,
        DateTime toDate,
        int topCount,
        CancellationToken cancellationToken = default)
    {
        return await GetBytesAsync(
            $"api/reports/top-selling-items/export/excel?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}&top={topCount}",
            cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentMethodRevenueDto>> GetRevenueByPaymentMethodAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<IReadOnlyList<PaymentMethodRevenueDto>>(
            $"api/reports/revenue/by-payment-method?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}",
            cancellationToken) ?? [];
    }

    public async Task<byte[]> ExportRevenueByPaymentMethodCsvAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await GetBytesAsync(
            $"api/reports/revenue/by-payment-method/export/csv?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}",
            cancellationToken);
    }

    public async Task<byte[]> ExportRevenueByPaymentMethodExcelAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await GetBytesAsync(
            $"api/reports/revenue/by-payment-method/export/excel?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}",
            cancellationToken);
    }

    public async Task<IReadOnlyList<PaidBillHistoryItemDto>> GetPaidBillHistoryAsync(
        PaidBillHistoryFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        string url = BuildPaidBillHistoryUrl("api/reports/paid-bill-history", filter);
        return await GetJsonAsync<IReadOnlyList<PaidBillHistoryItemDto>>(url, cancellationToken) ?? [];
    }

    public async Task<byte[]> ExportPaidBillHistoryCsvAsync(
        PaidBillHistoryFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        string url = BuildPaidBillHistoryUrl("api/reports/paid-bill-history/export/csv", filter);
        return await GetBytesAsync(url, cancellationToken);
    }

    public async Task<byte[]> ExportPaidBillHistoryExcelAsync(
        PaidBillHistoryFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        string url = BuildPaidBillHistoryUrl("api/reports/paid-bill-history/export/excel", filter);
        return await GetBytesAsync(url, cancellationToken);
    }

    public async Task<MenuCatalogDto> GetMenuCatalogAsync(
        string salesChannelCode = "DINE_IN",
        CancellationToken cancellationToken = default)
    {
        string channelCode = string.IsNullOrWhiteSpace(salesChannelCode)
            ? "DINE_IN"
            : Uri.EscapeDataString(salesChannelCode.Trim());

        return await _httpClient.GetFromJsonAsync<MenuCatalogDto>(
            $"api/staff/menu?salesChannelCode={channelCode}&availableOnly=true",
            cancellationToken) ?? new MenuCatalogDto();
    }

    public async Task<IReadOnlyList<BillSummaryDto>> GetBillsBySessionAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<BillSummaryDto>>(
            $"api/staff/bills/session/{tableSessionId}",
            cancellationToken) ?? [];
    }

    public async Task<BillDto?> GetBillAsync(int billId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<BillDto>($"api/staff/bills/{billId}", cancellationToken);
    }

    public async Task<BillDto> GetOrCreateDefaultBillAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"api/staff/bills/session/{tableSessionId}/default",
            content: null,
            cancellationToken);

        return await ReadSuccessAsync<BillDto>(response, cancellationToken);
    }

    public async Task<TableSessionDto> GetOrCreateTableSessionAsync(
        int tableId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"api/staff/tables/{tableId}/session",
            content: null,
            cancellationToken);

        return await ReadSuccessAsync<TableSessionDto>(response, cancellationToken);
    }

    public async Task<BillDto> CreateEmptyBillAsync(
        int tableSessionId,
        string billName,
        int? salesChannelId = null,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"api/staff/bills/session/{tableSessionId}/empty",
            new CreateEmptyBillApiRequest { BillName = billName, SalesChannelId = salesChannelId },
            cancellationToken);

        return await ReadSuccessAsync<BillDto>(response, cancellationToken);
    }

    public async Task<BillDto> AdjustBillDetailQuantityAsync(
        int billDetailId,
        int newQuantity,
        string? changeReason = null,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            $"api/staff/bills/details/{billDetailId}/quantity",
            new AdjustBillDetailQuantityRequest
            {
                BillDetailId = billDetailId,
                NewQuantity = newQuantity,
                RestoreStock = false,
                ChangeReason = changeReason
            },
            cancellationToken);

        return await ReadSuccessAsync<BillDto>(response, cancellationToken);
    }

    public async Task CancelBillAsync(
        int billId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Delete, $"api/staff/bills/{billId}");
        request.Content = JsonContent.Create(new CancelBillApiRequest { Reason = reason });

        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
            ? $"API request failed with HTTP {(int)response.StatusCode}."
            : body);
    }

    public async Task<BillDto> NotifyBillAsync(
        int billId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"api/staff/bills/{billId}/notify",
            content: null,
            cancellationToken);

        return await ReadSuccessAsync<BillDto>(response, cancellationToken);
    }

    public async Task<BillDto> RenameBillAsync(
        int billId,
        string billName,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            $"api/staff/bills/{billId}/name",
            new RenameBillRequest { BillName = billName },
            cancellationToken);

        return await ReadSuccessAsync<BillDto>(response, cancellationToken);
    }

    public async Task<PaymentResultDto> ConfirmCombinedPaymentAsync(
        ConfirmCombinedPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/staff/payments/confirm-combined",
            request,
            cancellationToken);

        return await ReadSuccessAsync<PaymentResultDto>(response, cancellationToken);
    }

    public async Task<PaymentDto> UpdatePaidPaymentMethodAsync(
        int billId,
        UpdatePaidPaymentMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            $"api/admin/payments/{billId}/method",
            request,
            cancellationToken);

        return await ReadSuccessAsync<PaymentDto>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentDto>> BatchUpdatePaymentsAsync(
        int billId,
        BatchUpdatePaymentsRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(
            $"api/admin/payments/{billId}/batch",
            request,
            cancellationToken);

        return await ReadSuccessAsync<IReadOnlyList<PaymentDto>>(response, cancellationToken);
    }

    public async Task<CancellationSummaryDto> GetCancellationSummaryAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        return await GetJsonAsync<CancellationSummaryDto>(
            $"api/reports/cancellations?date={date:yyyy-MM-dd}",
            cancellationToken) ?? new CancellationSummaryDto();
    }

    public async Task<DashboardAssistantChatResponseDto> ChatWithDashboardAssistantAsync(
        DashboardAssistantChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/reports/assistant/chat",
            request,
            cancellationToken);

        return await ReadSuccessAsync<DashboardAssistantChatResponseDto>(response, cancellationToken);
    }

    public async Task<BillDto> SplitBillBatchAsync(
        SplitBillBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/staff/bills/split-batch",
            request,
            cancellationToken);

        return await ReadSuccessAsync<BillDto>(response, cancellationToken);
    }

    public async Task<BillDto> MergeBillAsync(
        MergeBillRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/staff/bills/merge",
            request,
            cancellationToken);

        return await ReadSuccessAsync<BillDto>(response, cancellationToken);
    }

    public async Task<TableSessionDto> MoveTableAsync(
        int tableSessionId,
        int targetTableId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"api/staff/sessions/{tableSessionId}/move-table",
            new MoveTableSessionRequest { TargetTableId = targetTableId },
            cancellationToken);

        return await ReadSuccessAsync<TableSessionDto>(response, cancellationToken);
    }

    public async Task<CreateOrderResponse> CreateStaffOrderAsync(
        CreateStaffOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/staff/orders",
            request,
            cancellationToken);

        return await ReadSuccessAsync<CreateOrderResponse>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<OrderSummaryDto>>(
            "api/staff/orders?status=PendingConfirmation",
            cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> GetOrdersAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        string url = "api/staff/orders";
        List<string> queryParams = [];
        if (from.HasValue) queryParams.Add($"from={from.Value:yyyy-MM-ddTHH:mm:ss}");
        if (to.HasValue) queryParams.Add($"to={to.Value:yyyy-MM-ddTHH:mm:ss}");
        if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={status}");
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        return await _httpClient.GetFromJsonAsync<IReadOnlyList<OrderSummaryDto>>(url, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<ServiceRequestDto>> GetServiceRequestsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        string url = "api/staff/requests";
        List<string> queryParams = [];
        if (from.HasValue) queryParams.Add($"from={from.Value:yyyy-MM-ddTHH:mm:ss}");
        if (to.HasValue) queryParams.Add($"to={to.Value:yyyy-MM-ddTHH:mm:ss}");
        if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={status}");
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        return await _httpClient.GetFromJsonAsync<IReadOnlyList<ServiceRequestDto>>(url, cancellationToken) ?? [];
    }

    public async Task<OrderDetailDto?> GetOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<OrderDetailDto>($"api/staff/orders/{orderId}", cancellationToken);
    }

    public async Task<BillDto> ConfirmOrderAsync(
        int orderId,
        int? targetBillId = null,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"api/staff/orders/{orderId}/confirm",
            new ConfirmOrderRequest { TargetBillId = targetBillId },
            cancellationToken);

        return await ReadSuccessAsync<BillDto>(response, cancellationToken);
    }

    public async Task<OrderDetailDto> CancelOrderAsync(
        int orderId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"api/staff/orders/{orderId}/cancel",
            new CancelOrderRequest { Reason = reason },
            cancellationToken);

        return await ReadSuccessAsync<OrderDetailDto>(response, cancellationToken);
    }

    public async Task<ServiceRequestDto> ConfirmServiceRequestAsync(
        int requestId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"api/staff/requests/{requestId}/confirm",
            content: null,
            cancellationToken);

        return await ReadSuccessAsync<ServiceRequestDto>(response, cancellationToken);
    }

    public async Task<BillDto> ApplySalesChannelPricingAsync(
        int billId,
        int salesChannelId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"api/staff/bills/{billId}/apply-pricing/{salesChannelId}",
            content: null,
            cancellationToken);

        return await ReadSuccessAsync<BillDto>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<ManagedSalesChannelDto>> GetSalesChannelsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<ManagedSalesChannelDto>>(
            "api/staff/bills/channels",
            cancellationToken) ?? [];
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private static async Task<T> ReadSuccessAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            T? value = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
            return value ?? throw new InvalidOperationException("API response body is empty.");
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(ParseApiErrorMessage(body, response.StatusCode));
    }

    private async Task<T?> GetJsonAsync<T>(string url, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        return await ReadSuccessAsync<T>(response, cancellationToken);
    }

    private async Task<byte[]> GetBytesAsync(string url, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(ParseApiErrorMessage(body, response.StatusCode));
    }

    private static string BuildPaidBillHistoryUrl(string basePath, PaidBillHistoryFilterDto filter)
    {
        List<string> query = [
            $"fromDate={filter.FromDate:yyyy-MM-dd}",
            $"toDate={filter.ToDate:yyyy-MM-dd}"
        ];

        if (!string.IsNullOrWhiteSpace(filter.PaymentMethod))
        {
            query.Add($"paymentMethod={Uri.EscapeDataString(filter.PaymentMethod.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(filter.TableName))
        {
            query.Add($"tableName={Uri.EscapeDataString(filter.TableName.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(filter.Area))
        {
            query.Add($"area={Uri.EscapeDataString(filter.Area.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            query.Add($"keyword={Uri.EscapeDataString(filter.Keyword.Trim())}");
        }

        return $"{basePath}?{string.Join("&", query)}";
    }

    private static string ParseApiErrorMessage(string? body, System.Net.HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"API request failed with HTTP {(int)statusCode}.";
        }

        try
        {
            ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(body);
            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return error.Message;
            }
        }
        catch (JsonException)
        {
        }

        return body;
    }

    private sealed class CreateEmptyBillApiRequest
    {
        public string BillName { get; set; } = string.Empty;
        public int? SalesChannelId { get; set; }
    }

    private sealed class CancelBillApiRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    private sealed class ApiErrorResponse
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}
