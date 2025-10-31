using ClientManager.Shared.Messaging;

namespace ClientManager.Worker;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    readonly ILogger<Worker> _logger = logger;
    readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();

        await consumer.StartAsync(cancellationToken);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
