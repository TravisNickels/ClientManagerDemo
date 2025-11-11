using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ClientManager.Shared.Messaging;

public class MessagePublisher(
    IMessageBrokerFactory messageBrokerFactory,
    IRoutingConvention routingConvention,
    IMessageContextAccessor messageContextAccessor,
    MessagePublishPipeline messagePublishPipeline
) : IMessagePublisher
{
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;
    readonly IRoutingConvention _routingConvention = routingConvention;
    readonly IMessageContextAccessor _messageContextAccessor = messageContextAccessor;
    readonly LinkedList<ulong> outstandingConfirms = new();
    readonly MessagePublishPipeline _messagePublishPipeline = messagePublishPipeline;

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : IMessage
    {
        await _messagePublishPipeline.ExecuteAsync(message, FinalPublishMiddlewareAsync, cancellationToken);
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
            Type = typeof(T).Name,
        };

        ulong sequenceNumber = await channel.GetNextPublishSequenceNumberAsync();
        outstandingConfirms.AddLast(sequenceNumber);

        await channel.BasicPublishAsync(exchange, routingKey, mandatory: true, basicProperties: props, body, cancellationToken);
    }

    MessageEnvelope<T> CreateEnvelope<T>(T message)
    {
        var context = _messageContextAccessor.Current;
        var correlationId = context?.CorrelationId ?? Guid.NewGuid();
        return new MessageEnvelope<T> { Message = message, CorrelationId = correlationId };
    }
}
