using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.DataAccessObjects.Seed;
using DineFlow.Services;
using DineFlow.Services.Auth;
using DineFlow.WPFApp.ViewModels;
using DineFlow.WPFApp.Views;
using DineFlow.WPFApp.Features.Management.Tables;
using DineFlow.WPFApp.Features.Management.Menu;
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

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Development.json", optional: false)
            .Build();

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
        services.AddTransient<LoginWindow>();
        services.AddTransient<UserManagementView>();
        services.AddTransient<TableManagementView>();
        services.AddTransient<MenuManagementView>();
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
                break;
            }

            using IServiceScope mainScope = _serviceProvider.CreateScope();
            mainScope.ServiceProvider.GetRequiredService<MainWindow>().ShowDialog();

            if (currentUser.IsAuthenticated)
            {
                break;
            }
        }

        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
