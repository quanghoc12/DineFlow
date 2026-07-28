using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Auth;
using DineFlow.WPFApp.Services.Authorization;
using DineFlow.WPFApp.Services.Configuration;

namespace DineFlow.WPFApp.Services.Api;

public sealed class ApiUserService : IUserService, IDisposable
{
    private readonly HttpClient _httpClient;

    public ApiUserService()
    {
        _httpClient = ApiHttpClientFactory.Create();
    }

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<UserSummary>>(
            "api/staff/management/users",
            cancellationToken) ?? [];
    }

    public async Task CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await SendNoContentAsync(HttpMethod.Post, "api/staff/management/users", request, cancellationToken);
    }

    public async Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        await SendNoContentAsync(HttpMethod.Put, $"api/staff/management/users/{request.UserId}", request, cancellationToken);
    }

    public async Task SetActiveAsync(int userId, bool isActive, CancellationToken cancellationToken = default)
    {
        await SendNoContentAsync(HttpMethod.Patch, $"api/staff/management/users/{userId}/active", new { isActive }, cancellationToken);
    }

    public async Task ResetPasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        await SendNoContentAsync(
            HttpMethod.Post,
            $"api/staff/management/users/{userId}/reset-password",
            new { currentPassword, newPassword },
            cancellationToken);
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
}

internal static class ApiHttpClientFactory
{
    public static HttpClient Create()
    {
        HttpClient client = new()
        {
            BaseAddress = new Uri(AppClientSettings.ResolveApiBaseUrl().TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(45)
        };
        client.DefaultRequestHeaders.Add("X-User-Id", ApiClientSession.CurrentUserId.ToString());
        client.DefaultRequestHeaders.Add("X-User-Role", ApiClientSession.CurrentUserRole);
        return client;
    }

    public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        string message = string.IsNullOrWhiteSpace(body)
            ? $"API lỗi {(int)response.StatusCode}."
            : body;

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("message", out JsonElement messageElement))
            {
                message = messageElement.GetString() ?? message;
            }
            else if (document.RootElement.TryGetProperty("title", out JsonElement titleElement))
            {
                message = titleElement.GetString() ?? message;
            }
        }
        catch (JsonException)
        {
        }

        throw new InvalidOperationException(message);
    }
}
