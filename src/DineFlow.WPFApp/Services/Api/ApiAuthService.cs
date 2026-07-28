using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DineFlow.BusinessObjects.Auth;
using DineFlow.Services.Auth;
using DineFlow.WPFApp.Services.Configuration;

namespace DineFlow.WPFApp.Services.Api;

public sealed class ApiAuthService : IAuthService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly HttpClient _httpClient;

    public ApiAuthService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(AppClientSettings.ResolveApiBaseUrl().TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(45)
        };
    }

    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            "api/staff/auth/login",
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return Failed(await ReadErrorMessageAsync(response, cancellationToken));
        }

        if (!response.IsSuccessStatusCode)
        {
            string message = await ReadErrorMessageAsync(response, cancellationToken);
            return Failed(string.IsNullOrWhiteSpace(message)
                ? $"Không thể đăng nhập qua server ({(int)response.StatusCode})."
                : message);
        }

        StaffLoginResponse? loginResponse = await response.Content.ReadFromJsonAsync<StaffLoginResponse>(
            cancellationToken: cancellationToken);
        if (loginResponse is null)
        {
            return Failed("Server trả dữ liệu đăng nhập không hợp lệ.");
        }

        CurrentUser user = new()
        {
            UserId = loginResponse.UserId,
            Username = loginResponse.Username,
            FullName = loginResponse.FullName,
            Role = loginResponse.Role
        };

        _currentUserService.Login(user);
        return new LoginResult { IsSuccess = true, User = user };
    }

    public void Logout()
    {
        _currentUserService.Logout();
    }

    private static LoginResult Failed(string message) =>
        new() { ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Tên đăng nhập hoặc mật khẩu không đúng." : message };

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("message", out JsonElement message)
                ? message.GetString() ?? string.Empty
                : body;
        }
        catch (JsonException)
        {
            return body;
        }
    }

    private sealed class StaffLoginResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
