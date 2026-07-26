using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using DineFlow.Services.Common;

namespace DineFlow.Services.Reports;

internal sealed class DeepSeekChatClient : IDeepSeekChatClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public DeepSeekChatClient(IConfiguration configuration)
    {
        _apiKey = configuration["DeepSeek:ApiKey"]
            ?? Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
            ?? string.Empty;

        string baseUrl = configuration["DeepSeek:BaseUrl"] ?? "https://api.deepseek.com";
        _model = configuration["DeepSeek:Model"] ?? "deepseek-v4-flash";

        int timeoutSeconds = int.TryParse(configuration["DeepSeek:TimeoutSeconds"], out int configuredTimeout)
            ? configuredTimeout
            : 30;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
    }

    public async Task<string> CompleteAsync(
        IReadOnlyList<DeepSeekChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new BusinessException("DEEPSEEK_API_KEY_MISSING", "Chưa cấu hình DEEPSEEK_API_KEY cho chatbot AI.");
        }

        using HttpRequestMessage request = new(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = JsonContent.Create(new
        {
            model = _model,
            messages = messages.Select(x => new { role = x.Role, content = x.Content }).ToList(),
            stream = false,
            temperature = 0.2
        });

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new BusinessException(
                "DEEPSEEK_REQUEST_FAILED",
                string.IsNullOrWhiteSpace(body)
                    ? $"DeepSeek API lỗi HTTP {(int)response.StatusCode}."
                    : $"DeepSeek API lỗi HTTP {(int)response.StatusCode}: {body}");
        }

        using JsonDocument document = JsonDocument.Parse(body);
        JsonElement root = document.RootElement;
        JsonElement choices = root.GetProperty("choices");
        if (choices.GetArrayLength() == 0)
        {
            throw new BusinessException("DEEPSEEK_EMPTY_RESPONSE", "DeepSeek không trả về nội dung trả lời.");
        }

        string? content = choices[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return string.IsNullOrWhiteSpace(content)
            ? "DeepSeek không trả về nội dung phân tích."
            : content.Trim();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
