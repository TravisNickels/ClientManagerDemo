namespace ClientManager.Shared.Messaging;

public interface IHandlerMessage<T>
    where T : IMessage
{
    Task HandleAsync(T message, CancellationToken cancellationToken);
}
