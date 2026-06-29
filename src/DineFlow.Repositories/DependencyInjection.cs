using DineFlow.DataAccessObjects.Bills;
using DineFlow.DataAccessObjects.Menu;
using DineFlow.DataAccessObjects.Orders;
using DineFlow.DataAccessObjects.Requests;
using Microsoft.Extensions.DependencyInjection;
using DineFlow.Repositories.Bills;
using DineFlow.Repositories.Common;
using DineFlow.Repositories.Menu;
using DineFlow.Repositories.Orders;
using DineFlow.Repositories.Requests;
using DineFlow.DataAccessObjects.Auth;
using DineFlow.Repositories.Auth;
using DineFlow.DataAccessObjects.Tables;
using DineFlow.Repositories.Tables;

namespace DineFlow.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddDineFlowRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserDao, UserDao>();
        services.AddScoped<ITableManagementDao, TableManagementDao>();
        services.AddScoped<IMenuManagementDao, MenuManagementDao>();
        services.AddScoped<IBillDao, BillDao>();
        services.AddScoped<IPaymentDao, PaymentDao>();
        services.AddScoped<IMenuReadDao, MenuReadDao>();
        services.AddScoped<IOrderDao, OrderDao>();
        services.AddScoped<ITableSessionDao, TableSessionDao>();
        services.AddScoped<IServiceRequestDao, ServiceRequestDao>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ITableSessionRepository, TableSessionRepository>();
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
        services.AddScoped<IMenuReadRepository, MenuReadRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITableManagementRepository, TableManagementRepository>();
        services.AddScoped<IMenuManagementRepository, MenuManagementRepository>();

        return services;
    }
}
