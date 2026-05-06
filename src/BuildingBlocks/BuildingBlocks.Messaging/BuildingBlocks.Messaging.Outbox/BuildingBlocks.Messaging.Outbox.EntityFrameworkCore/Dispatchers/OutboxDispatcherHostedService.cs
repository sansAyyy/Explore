using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Options;

namespace BuildingBlocks.Messaging.Outbox.EntityFrameworkCore.Dispatchers;

public sealed class OutboxDispatcherHostedService<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly OutboxDispatcherOptions _options;
    private readonly ILogger<OutboxDispatcherHostedService<TDbContext>> _logger;

    public OutboxDispatcherHostedService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<OutboxDispatcherOptions> options,
        ILogger<OutboxDispatcherHostedService<TDbContext>> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<EntityFrameworkOutboxDispatcher<TDbContext>>();
                await dispatcher.DispatchBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unhandled outbox dispatch error. DbContext: {DbContextName}",
                    typeof(TDbContext).Name);
            }

            try
            {
                await Task.Delay(_options.PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}

