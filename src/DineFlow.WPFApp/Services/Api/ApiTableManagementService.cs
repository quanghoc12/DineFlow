using System.Net.Http;
using System.Net.Http.Json;
using DineFlow.BusinessObjects.Tables;
using DineFlow.Services.Tables;

namespace DineFlow.WPFApp.Services.Api;

public sealed class ApiTableManagementService : ITableManagementService, IDisposable
{
    private readonly HttpClient _httpClient = ApiHttpClientFactory.Create();

    public async Task<IReadOnlyList<ManagedTableDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<ManagedTableDto>>(
            "api/staff/management/tables",
            cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<ManagedAreaDto>> GetAreasAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<ManagedAreaDto>>(
            "api/staff/management/tables/areas",
            cancellationToken) ?? [];
    }

    public async Task<ManagedAreaDto> SaveAreaAsync(SaveAreaRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/staff/management/tables/areas",
            request,
            cancellationToken);
        return await ReadAsync<ManagedAreaDto>(response, cancellationToken);
    }

    public async Task SetAreaActiveAsync(int areaId, bool active, CancellationToken cancellationToken = default)
    {
        await SendNoContentAsync(HttpMethod.Patch, $"api/staff/management/tables/areas/{areaId}/active", new { isActive = active }, cancellationToken);
    }

    public async Task<ManagedTableDto> CreateAsync(CreateManagedTableRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/staff/management/tables",
            request,
            cancellationToken);
        return await ReadAsync<ManagedTableDto>(response, cancellationToken);
    }

    public async Task UpdateAsync(UpdateManagedTableRequest request, CancellationToken cancellationToken = default)
    {
        await SendNoContentAsync(HttpMethod.Put, $"api/staff/management/tables/{request.TableId}", request, cancellationToken);
    }

    public async Task SetActiveAsync(int tableId, bool active, CancellationToken cancellationToken = default)
    {
        await SendNoContentAsync(HttpMethod.Patch, $"api/staff/management/tables/{tableId}/active", new { isActive = active }, cancellationToken);
    }

    public async Task<ManagedTableDto> ResetQrAsync(int tableId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"api/staff/management/tables/{tableId}/reset-qr",
            content: null,
            cancellationToken);
        return await ReadAsync<ManagedTableDto>(response, cancellationToken);
    }

    public async Task<ManagedTableDto> ResetOtpAsync(int tableId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"api/staff/management/tables/{tableId}/reset-otp",
            content: null,
            cancellationToken);
        return await ReadAsync<ManagedTableDto>(response, cancellationToken);
    }

    public void Dispose() => _httpClient.Dispose();

    private async Task SendNoContentAsync<T>(HttpMethod method, string path, T body, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, path)
        {
            Content = JsonContent.Create(body)
        };
        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        await ApiHttpClientFactory.EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await ApiHttpClientFactory.EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken) ??
            throw new InvalidOperationException("API response body is empty.");
    }
}
