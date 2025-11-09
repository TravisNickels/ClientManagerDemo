namespace ClientManager.Shared.Messaging;

public interface IHandleMessage<T>
    where T : IMessage
{
    Task HandleAsync(T message, MessageContext context, CancellationToken cancellationToken);
}
