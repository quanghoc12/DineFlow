using DineFlow.Repositories;
using DineFlow.Services.Bills;
using DineFlow.Services.CustomerSessions;
using DineFlow.Services.Menu;
using DineFlow.Services.Orders;
using DineFlow.Services.Realtime;
using DineFlow.Services.Requests;
using DineFlow.Services.Auth;
using DineFlow.Services.Tables;
using Microsoft.Extensions.DependencyInjection;

namespace DineFlow.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddDineFlowServices(this IServiceCollection services)
    {
        services.AddDineFlowRepositories();
        services.AddScoped<IBillService, BillService>();
        services.AddScoped<IMenuCatalogService, MenuCatalogService>();
        services.AddScoped<ISplitBillService, SplitBillService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITableSessionService, TableSessionService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderPrintService, OrderPrintService>();
        services.AddScoped<IServiceRequestService, ServiceRequestService>();
        services.AddScoped<ICustomerSessionService, CustomerSessionService>();
        services.AddScoped<IRealtimeNotificationService, NullRealtimeNotificationService>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITableManagementService, TableManagementService>();
        services.AddScoped<IMenuManagementService, MenuManagementService>();

        return services;
    }
}
