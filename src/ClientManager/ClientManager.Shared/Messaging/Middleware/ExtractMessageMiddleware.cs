using System.Text.Json;

namespace ClientManager.Shared.Messaging;

public class ExtractMessageMiddleware : IMessageConsumeMiddleware
{
    readonly IReadOnlyDictionary<string, Type> _messageTypeCache = DiscoverMessageTypes().ToDictionary(t => t.Name, t => t);

    public async Task InvokeAsync(MessageConsumeContext context, MessageConsumeDelegate next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(context.MessageType, nameof(context.MessageType));
        ArgumentNullException.ThrowIfNull(context.Envelope, nameof(context.Envelope));

        var payload = context.Envelope.Payload;
        context.Message =
            payload.Deserialize(context.MessageType) ?? throw new InvalidOperationException($"Failed to deserialize message of type {context.MessageType.FullName}.");

        await next(context, cancellationToken);
    }

    static IEnumerable<Type> DiscoverMessageTypes() =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .ToList();
}
