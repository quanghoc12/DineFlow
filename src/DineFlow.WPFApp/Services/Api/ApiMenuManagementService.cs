using System.Net.Http;
using System.Net.Http.Json;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Services.Menu;

namespace DineFlow.WPFApp.Services.Api;

public sealed class ApiMenuManagementService : IMenuManagementService, IDisposable
{
    private readonly HttpClient _httpClient = ApiHttpClientFactory.Create();

    public async Task<IReadOnlyList<ManagedCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<IReadOnlyList<ManagedCategoryDto>>("api/staff/management/menu/categories", cancellationToken) ?? [];

    public async Task<IReadOnlyList<ManagedMenuItemDto>> GetItemsAsync(CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<IReadOnlyList<ManagedMenuItemDto>>("api/staff/management/menu/items", cancellationToken) ?? [];

    public Task SaveCategoryAsync(SaveCategoryRequest request, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Post, "api/staff/management/menu/categories", request, cancellationToken);

    public Task SetCategoryActiveAsync(int categoryId, bool active, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Patch, $"api/staff/management/menu/categories/{categoryId}/active", new { isActive = active }, cancellationToken);

    public Task DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"api/staff/management/menu/categories/{categoryId}", cancellationToken);

    public Task SaveItemAsync(SaveMenuItemRequest request, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Post, "api/staff/management/menu/items", request, cancellationToken);

    public Task SetItemAvailabilityAsync(int itemId, bool available, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Patch, $"api/staff/management/menu/items/{itemId}/availability", new { isAvailable = available }, cancellationToken);

    public Task SetItemDeletedAsync(int itemId, bool deleted, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Patch, $"api/staff/management/menu/items/{itemId}/deleted", new { isDeleted = deleted }, cancellationToken);

    public async Task<IReadOnlyList<ManagedChoiceGroupDto>> GetChoiceGroupsAsync(CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<IReadOnlyList<ManagedChoiceGroupDto>>("api/staff/management/menu/choice-groups", cancellationToken) ?? [];

    public Task SaveChoiceGroupAsync(SaveChoiceGroupRequest request, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Post, "api/staff/management/menu/choice-groups", request, cancellationToken);

    public Task SaveChoiceItemAsync(SaveChoiceItemRequest request, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Post, "api/staff/management/menu/choice-items", request, cancellationToken);

    public Task SetChoiceGroupAvailabilityAsync(int choiceGroupId, bool available, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Patch, $"api/staff/management/menu/choice-groups/{choiceGroupId}/availability", new { isAvailable = available }, cancellationToken);

    public Task SetChoiceItemAvailabilityAsync(int choiceItemId, bool available, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Patch, $"api/staff/management/menu/choice-items/{choiceItemId}/availability", new { isAvailable = available }, cancellationToken);

    public Task AssignChoiceGroupAsync(AssignChoiceGroupRequest request, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Post, "api/staff/management/menu/choice-group-assignments", request, cancellationToken);

    public Task RemoveChoiceGroupAssignmentAsync(int menuItemId, int choiceGroupId, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"api/staff/management/menu/items/{menuItemId}/choice-groups/{choiceGroupId}", cancellationToken);

    public async Task<IReadOnlyList<ManagedSalesChannelDto>> GetSalesChannelsAsync(CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<IReadOnlyList<ManagedSalesChannelDto>>("api/staff/management/menu/sales-channels", cancellationToken) ?? [];

    public Task SaveSalesChannelAsync(SaveSalesChannelRequest request, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Post, "api/staff/management/menu/sales-channels", request, cancellationToken);

    public Task SetSalesChannelActiveAsync(int salesChannelId, bool active, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Patch, $"api/staff/management/menu/sales-channels/{salesChannelId}/active", new { isActive = active }, cancellationToken);

    public Task DeleteSalesChannelAsync(int salesChannelId, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Delete, $"api/staff/management/menu/sales-channels/{salesChannelId}", cancellationToken);

    public Task SaveMenuItemChannelPriceAsync(SaveChannelPriceRequest request, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Post, "api/staff/management/menu/item-channel-prices", request, cancellationToken);

    public Task SaveChoiceItemChannelPriceAsync(SaveChannelPriceRequest request, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(HttpMethod.Post, "api/staff/management/menu/choice-item-channel-prices", request, cancellationToken);

    public void Dispose() => _httpClient.Dispose();

    private async Task SendNoContentAsync(HttpMethod method, string path, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, path);
        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        await ApiHttpClientFactory.EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task SendNoContentAsync<T>(HttpMethod method, string path, T body, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, path)
        {
            Content = JsonContent.Create(body)
        };
        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        await ApiHttpClientFactory.EnsureSuccessAsync(response, cancellationToken);
    }
}
