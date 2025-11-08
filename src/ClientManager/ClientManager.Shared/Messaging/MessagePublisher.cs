using RabbitMQ.Client;

namespace ClientManager.Shared.Messaging;

public class MessagePublisher(IMessageBrokerFactory messageBrokerFactory) : IMessagePublisher
{
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;
    readonly LinkedList<ulong> outstandingConfirms = new();

    public async Task PublishAsync(string queueName, ReadOnlyMemory<byte> body, string exchange = "", string routingKey = "")
    {
        exchange = string.IsNullOrWhiteSpace(exchange) ? "client-manager" : exchange;
        routingKey = string.IsNullOrWhiteSpace(routingKey) ? queueName : routingKey;
        var channel = await _messageBrokerFactory.GetPublishChannelAsync(exchange);

        var props = new BasicProperties();
        ulong sequenceNumber = await channel.GetNextPublishSequenceNumberAsync();
        outstandingConfirms.AddLast(sequenceNumber);

        await channel.BasicPublishAsync(exchange, routingKey, mandatory: true, basicProperties: props, body);
    }
}
