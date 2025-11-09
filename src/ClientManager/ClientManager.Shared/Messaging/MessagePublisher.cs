using RabbitMQ.Client;

namespace ClientManager.Shared.Messaging;

public class MessagePublisher(IMessageBrokerFactory messageBrokerFactory) : IMessagePublisher
{
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;
    readonly IRoutingConvention _routingConvention = routingConvention;
    readonly IMessageContextAccessor _messageContextAccessor = messageContextAccessor;
    readonly LinkedList<ulong> outstandingConfirms = new();

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : IMessage
    {
        exchange = string.IsNullOrWhiteSpace(exchange) ? "client-manager" : exchange;
        routingKey = string.IsNullOrWhiteSpace(routingKey) ? queueName : routingKey;
        var (exchange, routingKey) = _routingConvention.ResolveFor(typeof(T));
        var channel = await _messageBrokerFactory.GetPublishChannelAsync(exchange);

        var props = new BasicProperties();
        ulong sequenceNumber = await channel.GetNextPublishSequenceNumberAsync();
        outstandingConfirms.AddLast(sequenceNumber);

        await channel.BasicPublishAsync(exchange, routingKey, mandatory: true, basicProperties: props, body);
    }
}
