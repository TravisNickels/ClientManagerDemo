using ClientManager.Shared.Contracts.Commands;
using Microsoft.Extensions.Logging;

namespace ClientManager.Shared.Messaging;

public class ContextEnrichmentMiddleware(IMessageContextAccessor messageContextAccessor, ILogger<ContextEnrichmentMiddleware>? logger) : IMessagePublishMiddleware
{
    readonly IMessageContextAccessor _messageContextAccessor = messageContextAccessor;
    readonly ILogger<ContextEnrichmentMiddleware>? _logger = logger;

    public async Task InvokeAsync<T>(T message, MessagePublishDeleagte<T> next, CancellationToken cancellationToken)
        where T : IMessage
    {
        if (message is CreateClient createClientMessage && createClientMessage.Id == Guid.Empty)
        {
            createClientMessage.Id = Guid.NewGuid();
            _logger?.LogDebug("Assigned new Id {Id} to mesage {MessageType}", createClientMessage.Id, typeof(T).Name);
            _messageContextAccessor.SetCausationId(createClientMessage.Id);
        }

        await next(message, cancellationToken);
    }
}
