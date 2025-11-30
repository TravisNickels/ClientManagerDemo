using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ClientManager.Shared.Messaging;

public class MessagePublisher(
    IMessageBrokerFactory messageBrokerFactory,
    IRoutingConvention routingConvention,
    IMessageContextAccessor messageContextAccessor,
    MessagePublishPipeline messagePublishPipeline,
    ILogger<MessagePublisher>? logger
) : IMessagePublisher
{
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;
    readonly IRoutingConvention _routingConvention = routingConvention;
    readonly IMessageContextAccessor _messageContextAccessor = messageContextAccessor;
    readonly MessagePublishPipeline _messagePublishPipeline = messagePublishPipeline;
    readonly ILogger<MessagePublisher>? _logger = logger;
    readonly LinkedList<ulong> outstandingConfirms = new();

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : IMessage
    {
        var context = _messageContextAccessor.GetOrCreateContext();
        try
        {
            _logger?.LogDebug("Starting publish for message {MessageType} with CorrelationId {CorrelationId}", typeof(T).Name, context.CorrelationId);

            await _messagePublishPipeline.ExecuteAsync(message, FinalPublishMiddlewareAsync, cancellationToken);

            _logger?.LogInformation("Successfully published message {MessageType}", typeof(T).Name);
        }
        catch (Exception)
        {
            _logger?.LogError("Error publishing message {MessageType}", typeof(T).Name);
            throw;
        }
        finally
        {
            _messageContextAccessor.ClearContext();

            _logger?.LogDebug("Message context cleared for {MessageType}", typeof(T).Name);
        }
    }

    public async Task FinalPublishMiddlewareAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : IMessage
    {
        var (exchange, routingKey) = _routingConvention.ResolveFor(typeof(T));
        var channel = await _messageBrokerFactory.GetPublishChannelAsync(exchange);

        var envelope = CreateEnvelope(message);

        var serializedMessage = JsonSerializer.Serialize(envelope);
        var body = Encoding.UTF8.GetBytes(serializedMessage);

        var props = new BasicProperties
        {
            MessageId = envelope.EnvelopeId.ToString(),
            CorrelationId = envelope.CorrelationId.ToString(),
            Timestamp = new AmqpTimestamp(envelope.CreatedUtc.ToUnixTimeSeconds()),
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>(),
            Type = typeof(T).Name,
        };

        ulong sequenceNumber = await channel.GetNextPublishSequenceNumberAsync(cancellationToken);
        props.Headers["x-publish-sequence"] = sequenceNumber.ToString();
        outstandingConfirms.AddLast(sequenceNumber);

        await channel.BasicPublishAsync(exchange, routingKey, mandatory: true, basicProperties: props, body, cancellationToken);
    }

    MessageEnvelope CreateEnvelope(IMessage message)
    {
        if (_messageContextAccessor.Current is null)
            throw new NullReferenceException("Current context accessor is not set.  Call GetOrCreateContext() first.");

        var context = _messageContextAccessor.Current;
        var correlationId = context!.CorrelationId;
        var causationId = context.CausationId ?? Guid.Empty;
        return new MessageEnvelope
        {
            MessageType = message.GetType().Name,
            Payload = JsonSerializer.SerializeToElement(message, message.GetType()),
            CorrelationId = correlationId,
            CausationId = causationId
        };
    }
}
