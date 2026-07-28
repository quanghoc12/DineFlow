using DineFlow.DataAccessObjects.DbContexts;
using DineFlow.DataAccessObjects.Seed;
using DineFlow.Api.Middleware;
using DineFlow.Api.Hubs;
using DineFlow.Api.Realtime;
using DineFlow.Api.BackgroundServices;
using DineFlow.Api.Services;
using DineFlow.Services;
using DineFlow.Services.Realtime;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5433;Database=dineflow;Username=dineflow_user;Password=dineflow_password";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null)));

builder.Services.AddDineFlowServices();
builder.Services.AddSingleton<IStaffAuthTokenService, StaffAuthTokenService>();
builder.Services.AddHttpClient<IMenuImageStorageService, MenuImageStorageService>();
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

if (builder.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await dbContext.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector");
        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS assistant_vector_contexts (
                id SERIAL PRIMARY KEY,
                source_type VARCHAR(80) NOT NULL,
                source_id INTEGER NOT NULL,
                occurred_at TIMESTAMP NOT NULL,
                title VARCHAR(300) NOT NULL,
                content TEXT NOT NULL,
                embedding vector(128) NOT NULL,
                created_at TIMESTAMP NOT NULL DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS ix_assistant_vector_contexts_embedding
                ON assistant_vector_contexts USING ivfflat (embedding vector_cosine_ops) WITH (lists = 50);
            CREATE UNIQUE INDEX IF NOT EXISTS ux_assistant_vector_contexts_source
                ON assistant_vector_contexts (source_type, source_id);
            """);
    }
    catch
    {
        // pgvector is optional for the assistant; JSON analytics and local semantic fallback still work without it.
    }

    await dbContext.Database.MigrateAsync();
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
