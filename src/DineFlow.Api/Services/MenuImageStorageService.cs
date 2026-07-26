using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace DineFlow.Api.Services;

public interface IMenuImageStorageService
{
    Task<MenuImageUploadResult> UploadMenuImageAsync(IFormFile file, CancellationToken cancellationToken = default);
}

public sealed class MenuImageStorageService : IMenuImageStorageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp"
    };

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public MenuImageStorageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<MenuImageUploadResult> UploadMenuImageAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        string supabaseUrl = GetRequiredSetting("SUPABASE_URL").TrimEnd('/');
        string serviceRoleKey = GetRequiredSetting("SUPABASE_SERVICE_ROLE_KEY");
        string bucket = GetRequiredSetting("SUPABASE_STORAGE_BUCKET");

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        string contentType = AllowedExtensions[extension];
        string objectPath = $"menu-items/{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid():N}{extension}";
        string uploadUrl = $"{supabaseUrl}/storage/v1/object/{Uri.EscapeDataString(bucket)}/{objectPath}";

        using Stream fileStream = file.OpenReadStream();
        using StreamContent content = new(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using HttpRequestMessage request = new(HttpMethod.Post, uploadUrl)
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceRoleKey);
        request.Headers.Add("apikey", serviceRoleKey);
        request.Headers.Add("x-upsert", "false");

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new MenuImageUploadException("Không thể tải ảnh lên. Vui lòng thử lại.");
        }

        string publicUrl = $"{supabaseUrl}/storage/v1/object/public/{Uri.EscapeDataString(bucket)}/{objectPath}";
        return new MenuImageUploadResult(publicUrl);
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new MenuImageUploadException("File ảnh không được để trống.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new MenuImageUploadException("Ảnh không được vượt quá 5MB.");
        }

        string extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.ContainsKey(extension))
        {
            throw new MenuImageUploadException("Định dạng ảnh không được hỗ trợ.");
        }
    }

    private string GetRequiredSetting(string key)
    {
        string? value = _configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new MenuImageUploadException($"Thiếu cấu hình {key}.");
        }

        return value.Trim();
    }
}

public sealed record MenuImageUploadResult(string ImageUrl);

public sealed class MenuImageUploadException : Exception
{
    public MenuImageUploadException(string message) : base(message)
    {
    }
}
