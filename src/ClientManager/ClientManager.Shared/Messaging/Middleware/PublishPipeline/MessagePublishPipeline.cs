namespace ClientManager.Shared.Messaging;

public class MessagePublishPipeline
{
    readonly List<IMessagePublishMiddleware> _messagePublishMiddlewares = [];

    public Task ExecuteAsync<T>(T message, MessagePublishDeleagte<T> finalHandler, CancellationToken cancellationToken = default)
        where T : IMessage
    {
        MessagePublishDeleagte<T> next = finalHandler;

        foreach (var middleware in _messagePublishMiddlewares.AsEnumerable().Reverse())
        {
            var current = middleware;
            var nextCopy = next;
            next = (msg, token) => current.InvokeAsync(msg, nextCopy, token);
        }

        return next(message, cancellationToken);
    }

    public MessagePublishPipeline Use(IMessagePublishMiddleware middleware)
    {
        _messagePublishMiddlewares.Add(middleware);
        return this;
    }
}
