using ClientManager.Shared.Messaging;

namespace ClientManager.Worker;

public class Worker(ILogger<Worker> logger, IMessageConsumer messageConsumer) : BackgroundService
{
    readonly ILogger<Worker> _logger = logger;
    readonly IMessageConsumer _messageConsumer = messageConsumer;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var tempPath = Path.GetTempPath();
        var healthyFile = Path.Combine(tempPath, "healthy");

        try
        {
            await _messageConsumer.StartAsync(cancellationToken);

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            // For docker health checks
            File.WriteAllText(healthyFile, "ready");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker failed startup.");
            File.Delete(healthyFile);
        }

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
