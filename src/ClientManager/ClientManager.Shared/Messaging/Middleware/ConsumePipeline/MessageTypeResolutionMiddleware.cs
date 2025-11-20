namespace ClientManager.Shared.Messaging;

public class MessageTypeResolutionMiddleware(MessageTypeRegistry messageTypeRegistry) : IMessageConsumeMiddleware
{
    readonly MessageTypeRegistry _messageTypeRegistry = messageTypeRegistry;

    public async Task InvokeAsync(MessageConsumeContext context, MessageConsumeDelegate next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context.Envelope, nameof(context.Envelope));

        var typeName = context.Envelope.MessageType;
        var resolvedType = _messageTypeRegistry.GetMessageTypeByName(typeName) ?? throw new InvalidOperationException($"No message type found for name: {typeName}");
        context.MessageType = resolvedType;
        await next(context, cancellationToken);
    }
}
