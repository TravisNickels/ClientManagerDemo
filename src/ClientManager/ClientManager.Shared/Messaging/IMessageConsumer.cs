namespace ClientManager.Shared.Messaging;

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
}
