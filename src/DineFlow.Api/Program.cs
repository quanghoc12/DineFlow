using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.DataAccessObjects.Seed;
using DineFlow.Api.Middleware;
using DineFlow.Api.Hubs;
using DineFlow.Api.Realtime;
using DineFlow.Api.BackgroundServices;
using DineFlow.Services;
using DineFlow.Services.Realtime;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5433;Database=dineflow;Username=dineflow_user;Password=dineflow_password";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDineFlowServices();
builder.Services.AddScoped<IRealtimeNotificationService, SignalRRealtimeNotificationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CustomerWeb", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddHostedService<BrowsingSessionExpirationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DevelopmentDataSeeder.SeedDevelopmentDataAsync(dbContext);
}

app.UseMiddleware<BusinessExceptionMiddleware>();
app.UseCors("CustomerWeb");

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "DineFlow.Api"
}));

app.MapControllers();
app.MapHub<DineFlowHub>("/hubs/dineflow");

app.Run();
