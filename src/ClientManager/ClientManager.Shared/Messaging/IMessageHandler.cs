namespace ClientManager.Shared.Messaging;

public interface IMessageHandler<T>
{
    Task HandleAsync(T message, CancellationToken cancellationToken);
}
