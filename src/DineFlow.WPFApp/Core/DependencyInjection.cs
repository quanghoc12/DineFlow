using DineFlow.DataAccessObjects;
using DineFlow.DataAccessObjects.Auth;
using DineFlow.Repositories.Impl;
using DineFlow.Repositories.Interfaces;
using DineFlow.Services.Impl;
using DineFlow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DineFlow.WPFApp.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddDineFlowServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // 2. DAOs
        services.AddScoped<UserDAO>();
        services.AddScoped<RoleDAO>();

        // 3. Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // 4. Services (Singletons)
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        services.AddSingleton<DineFlow.WPFApp.Services.INavigationService, DineFlow.WPFApp.Services.NavigationService>();
        services.AddScoped<DineFlow.WPFApp.Services.IDialogService, DineFlow.WPFApp.Services.DialogService>();

        // 6. ViewModels
        services.AddTransient<DineFlow.WPFApp.ViewModels.LoginViewModel>();
        services.AddTransient<DineFlow.WPFApp.ViewModels.MainViewModel>();
        services.AddTransient<DineFlow.WPFApp.ViewModels.UserManagementViewModel>();
        services.AddTransient<DineFlow.WPFApp.ViewModels.UserEditorViewModel>();
        services.AddTransient<DineFlow.WPFApp.ViewModels.ResetPasswordViewModel>();

        // 7. Windows
        services.AddTransient<DineFlow.WPFApp.Views.LoginWindow>();
        services.AddTransient<DineFlow.WPFApp.Views.MainWindow>();
        services.AddTransient<DineFlow.WPFApp.Views.UserEditorWindow>();
        services.AddTransient<DineFlow.WPFApp.Views.ResetPasswordWindow>();

        return services;
    }
}
