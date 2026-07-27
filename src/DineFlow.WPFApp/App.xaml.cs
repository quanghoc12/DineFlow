using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.DataAccessObjects.Seed;
using DineFlow.Services;
using DineFlow.Services.Auth;
using DineFlow.WPFApp.Features.Auth;
using DineFlow.WPFApp.Features.Management.Users;
using DineFlow.WPFApp.Features.Management.Tables;
using DineFlow.WPFApp.Features.Management.Menu;
using DineFlow.WPFApp.Features.Reports.Dashboard;
using DineFlow.WPFApp.Features.Reports.Revenue;
using DineFlow.WPFApp.Features.Reports.Cancellation;
using DineFlow.WPFApp.Services.Authorization;
using DineFlow.WPFApp.Services.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace DineFlow.WPFApp;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IConfiguration configuration = AppClientSettings.LoadConfiguration();

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection.");

        ServiceCollection services = new();
        services.AddSingleton(configuration);
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDineFlowServices();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<UserManagementViewModel>();
        services.AddTransient<TableManagementViewModel>();
        services.AddTransient<MenuManagementViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddScoped<DashboardChatSessionStore>();
        services.AddScoped<DashboardAssistantViewModel>();
        services.AddTransient<RevenueReportViewModel>();
        services.AddTransient<CancellationViewModel>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<UserManagementView>();
        services.AddTransient<TableManagementView>();
        services.AddTransient<MenuManagementView>();
        services.AddTransient<DashboardView>();
        services.AddTransient<RevenueReportView>();
        services.AddTransient<CancellationView>();
        services.AddTransient<MainWindow>();
        _serviceProvider = services.BuildServiceProvider();

        using (IServiceScope initializationScope = _serviceProvider.CreateScope())
        {
            AppDbContext dbContext = initializationScope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            await DevelopmentDataSeeder.SeedDevelopmentDataAsync(dbContext);
        }

        RunLoginLoop();
    }

    private void RunLoginLoop()
    {
        if (_serviceProvider is null)
        {
            Shutdown();
            return;
        }

        ICurrentUserService currentUser = _serviceProvider.GetRequiredService<ICurrentUserService>();

        while (true)
        {
            using IServiceScope loginScope = _serviceProvider.CreateScope();
            bool authenticated = loginScope.ServiceProvider
                .GetRequiredService<LoginWindow>()
                .ShowDialog() == true;

            if (!authenticated || !currentUser.IsAuthenticated)
            {
                ApiClientSession.Clear();
                break;
            }

            ApiClientSession.Configure(currentUser.User);

            using IServiceScope mainScope = _serviceProvider.CreateScope();
            mainScope.ServiceProvider.GetRequiredService<MainWindow>().ShowDialog();

            if (currentUser.IsAuthenticated)
            {
                break;
            }

            ApiClientSession.Clear();
        }

        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
