namespace ClientManager.Shared.Messaging;

public class MessagePublisherPipeline(IEnumerable<IMessagePublishMiddleware> messagePublishMiddlewares)
{
    IEnumerable<IMessagePublishMiddleware> _messagePublishMiddlewares = messagePublishMiddlewares.ToList();

    public Task ExecuteAsync<T>(T message, MessagePublishDeleagte<T> finalHandler, CancellationToken cancellationToken)
        where T : IMessage
    {
        MessagePublishDeleagte<T> next = finalHandler;

        foreach (var middleware in _messagePublishMiddlewares.Reverse())
        {
            var current = middleware;
            var nextCopy = next;
            next = (msg, token) => current.InvokeAsync(msg, nextCopy, token);
        }

        return next(message, cancellationToken);
    }
}
