using Microsoft.Extensions.Configuration;

namespace DineFlow.WPFApp.Services;

internal static class AppClientSettings
{
    private const string DefaultApiBaseUrl = "http://localhost:5080";

    public static IConfiguration LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static string ResolveApiBaseUrl()
    {
        string? configuredUrl = LoadConfiguration()["Api:BaseUrl"]?.Trim();
        return string.IsNullOrWhiteSpace(configuredUrl)
            ? DefaultApiBaseUrl
            : configuredUrl.TrimEnd('/');
    }
}
