using ClientManager.Shared.Messaging;

namespace ClientManager.Worker;

public class Worker(ILogger<Worker> logger, IMessageConsumer messageConsumer) : BackgroundService
{
    readonly ILogger<Worker> _logger = logger;
    readonly IMessageConsumer _messageConsumer = messageConsumer;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _messageConsumer.StartAsync(cancellationToken);

        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
