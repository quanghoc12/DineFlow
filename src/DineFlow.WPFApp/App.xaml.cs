using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Views;
using DineFlow.Services.Interfaces;

namespace DineFlow.WPFApp;

public partial class App : Application
{
    private IServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global Exception Handling
        this.DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"Lỗi giao diện: {args.Exception.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfiguration configuration = builder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddDineFlowServices(configuration);
        
        _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        // Ensure database is created and seeded
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DineFlow.DataAccessObjects.AppDbContext>();
                
                // Ensure the database is created if it doesn't exist
                dbContext.Database.EnsureCreated();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể kết nối CSDL: {ex.Message}", "Lỗi Khởi Động", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Start Application Orchestration Loop
        RunOrchestrationLoop();
    }

    private void RunOrchestrationLoop()
    {
        var currentUserService = _serviceProvider.GetRequiredService<ICurrentUserService>();

        while (true)
        {
            bool loginSucceeded = false;
            using (var scope = _serviceProvider.CreateScope())
            {
                var loginWindow = scope.ServiceProvider.GetRequiredService<LoginWindow>();
                var loginResult = loginWindow.ShowDialog();

                loginSucceeded = loginResult == true && currentUserService.IsAuthenticated();
            }

            // If user closed the window without a successful login, shutdown the app
            if (!loginSucceeded)
            {
                break;
            }

            // User is authenticated, show Main Window
            using (var scope = _serviceProvider.CreateScope())
            {
                var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.ShowDialog(); // Blocks until window closes
            }

            // When Main Window closes, check if it was due to a logout
            if (!currentUserService.IsAuthenticated())
            {
                // Restart loop and show a fresh LoginWindow
                continue;
            }

            // Natural close
            break;
        }

        // Clean up singleton instances and scopes safely
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Shutdown();
    }
}
