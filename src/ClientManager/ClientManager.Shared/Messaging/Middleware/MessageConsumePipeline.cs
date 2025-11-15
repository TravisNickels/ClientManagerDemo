namespace ClientManager.Shared.Messaging;

public class MessageConsumePipeline
{
    private readonly List<IMessageConsumeMiddleware> _messageConsumeMiddlewares = [];

    public Task ExecuteAsync(MessageConsumeContext message, MessageConsumeDelegate finalHandler, CancellationToken cancellationToken = default)
    {
        MessageConsumeDelegate next = finalHandler;

        foreach (var middleware in _messageConsumeMiddlewares.AsEnumerable().Reverse())
        {
            var current = middleware;
            var nextCopy = next;
            next = (msg, ct) => current.InvokeAsync(msg, nextCopy, ct);
        }
        return next(message, cancellationToken);
    }

    public MessageConsumePipeline Use(IMessageConsumeMiddleware middleware)
    {
        _messageConsumeMiddlewares.Add(middleware);
        return this;
    }
}
