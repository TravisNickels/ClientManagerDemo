using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Serilog.Context;

namespace ClientManager.Shared.Messaging;

public class MessagePublisher(IMessageBrokerFactory messageBrokerFactory, IRoutingConvention routingConvention, IMessageContextAccessor messageContextAccessor)
    : IMessagePublisher
{
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;
    readonly IRoutingConvention _routingConvention = routingConvention;
    readonly IMessageContextAccessor _messageContextAccessor = messageContextAccessor;
    readonly LinkedList<ulong> outstandingConfirms = new();

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : IMessage
    {
        var (exchange, routingKey) = _routingConvention.ResolveFor(typeof(T));
        var channel = await _messageBrokerFactory.GetPublishChannelAsync(exchange);

        var envelope = CreateEnvelope(message);

        var serializedMessage = JsonSerializer.Serialize(envelope);
        var body = Encoding.UTF8.GetBytes(serializedMessage);

        var props = new BasicProperties
        {
            MessageId = envelope.MessageId.ToString(),
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
