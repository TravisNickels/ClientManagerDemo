using System.Text.Json;

namespace ClientManager.Shared.Messaging;

public class ExtractMessageMiddleware : IMessageConsumeMiddleware
{
    public async Task InvokeAsync(MessageConsumeContext context, MessageConsumeDelegate next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(context.Envelope, nameof(context.Envelope));
        ArgumentNullException.ThrowIfNull(context.MessageType, nameof(context.MessageType));

        var payload = context.Envelope.Payload;
        context.Message =
            payload.Deserialize(context.MessageType) ?? throw new InvalidOperationException($"Failed to deserialize message of type {context.MessageType.FullName}.");

        await next(context, cancellationToken);
    }
}
