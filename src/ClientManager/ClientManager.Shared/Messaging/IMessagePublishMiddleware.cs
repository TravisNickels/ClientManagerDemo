namespace ClientManager.Shared.Messaging;

public interface IMessagePublishMiddleware
{
    Task InvokeAsync<T>(T message, MessagePublishDeleagte<T> next, CancellationToken cancellationToken)
        where T : IMessage;
}

public delegate Task MessagePublishDeleagte<T>(T message, CancellationToken cancellationToken = default)
    where T : IMessage;
