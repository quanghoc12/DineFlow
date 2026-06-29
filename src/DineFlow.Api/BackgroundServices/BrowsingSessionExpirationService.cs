using DineFlow.Services.Orders;

namespace DineFlow.Api.BackgroundServices;

public sealed class BrowsingSessionExpirationService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BrowsingSessionExpirationService> _logger;

    public BrowsingSessionExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<BrowsingSessionExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(CheckInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ExpireSessionsAsync(stoppingToken);

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }

    private async Task ExpireSessionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            ITableSessionService service = scope.ServiceProvider.GetRequiredService<ITableSessionService>();
            int expiredCount = await service.ExpireInactiveBrowsingSessionsAsync(cancellationToken);

            if (expiredCount > 0)
            {
                _logger.LogInformation("Expired {ExpiredCount} inactive browsing sessions.", expiredCount);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to expire inactive browsing sessions.");
        }
    }
}
