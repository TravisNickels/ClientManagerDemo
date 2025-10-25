using RabbitMQ.Client;

namespace ClientManager.Shared.Messaging;

public interface IMessageBroker
{
    ValueTask<IConnection> GetConnectionAsync();
    ValueTask<IChannel> GetChannelAsync(string channelName, CreateChannelOptions? options);
    Task SendToQueue(string queueName, ReadOnlyMemory<byte> body, string exchange, string routingKey);
}
