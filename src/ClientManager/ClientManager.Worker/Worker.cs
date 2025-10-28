using ClientManager.Shared.Messaging;
using ClientManager.Worker.Repositories;

namespace ClientManager.Worker;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    readonly ILogger<Worker> _logger = logger;
    readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();

        await consumer.StartAsync(cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, cancellationToken);
        }
    }
}
