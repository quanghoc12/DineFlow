using Microsoft.Extensions.Configuration;

namespace DineFlow.WPFApp.Services.Configuration;

internal static class AppClientSettings
{
    private const string DefaultApiBaseUrl = "https://dineflow-ohjj.onrender.com";

    public static IConfiguration LoadConfiguration()
    {
        string environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Production";

        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true);

        if (environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddJsonFile("appsettings.Development.json", optional: true);
        }

        builder.AddEnvironmentVariables();
        return builder.Build();
    }

    public static string ResolveApiBaseUrl()
    {
        string? configuredUrl = LoadConfiguration()["Api:BaseUrl"]?.Trim();
        return string.IsNullOrWhiteSpace(configuredUrl)
            ? DefaultApiBaseUrl
            : configuredUrl.TrimEnd('/');
    }
}
