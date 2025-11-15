using System.Text.Json;

namespace ClientManager.Shared.Messaging.Middleware;

public class ExtractEnvelopeMiddleware : IMessageConsumeMiddleware
{
    public async Task InvokeAsync(MessageConsumeContext context, MessageConsumeDelegate next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var deserializedEnvelope =
            JsonSerializer.Deserialize<MessageEnvelope>(context.RawJson) ?? throw new InvalidOperationException("Failed to deserialize message envelope.");

        context.Envelope = deserializedEnvelope;

        await next(context, cancellationToken);
    }
}
