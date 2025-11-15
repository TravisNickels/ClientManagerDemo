using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClientManager.Shared.Messaging;

public class ScopeMessageContextMiddleware(IServiceScopeFactory serviceScopeFactory, ILogger<ScopeMessageContextMiddleware> logger) : IMessageConsumeMiddleware
{
    readonly IServiceScopeFactory _scopeFactory = serviceScopeFactory;
    readonly ILogger<ScopeMessageContextMiddleware> _logger = logger;

    public async Task InvokeAsync(MessageConsumeContext context, MessageConsumeDelegate next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context.Envelope, nameof(context.Envelope));
        ArgumentNullException.ThrowIfNull(context.MessageType, nameof(context.MessageType));

        var scope = _scopeFactory.CreateAsyncScope();

        var messageContextAccessor = scope.ServiceProvider.GetRequiredService<IMessageContextAccessor>();

        context.MessageContext = GetMessageContext(context.Envelope);
        messageContextAccessor.SetCurrentContext(context.MessageContext);

        context.Scope = scope;

        _logger.LogInformation(
            "Received message from {messageType}: \n\t[correlationId: {correlationId}]\n\t{@Message}",
            context.MessageType.Name,
            context.MessageContext.CorrelationId,
            context.Message
        );

        try
        {
            await next(context, cancellationToken);
        }
        finally
        {
            await scope.DisposeAsync();
            context.Scope = null;
        }
    }

    static MessageContext GetMessageContext(MessageEnvelope envelope) =>
        new MessageContext(CorrelationId: envelope.CorrelationId, CausationId: envelope.CausationId, Timestamp: envelope.CreatedUtc);
}
