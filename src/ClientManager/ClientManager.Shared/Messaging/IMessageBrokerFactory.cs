using RabbitMQ.Client;

namespace ClientManager.Shared.Messaging;

public interface IMessageBrokerFactory : IAsyncDisposable
{
    ValueTask<IConnection> GetConnectionAsync();
    ValueTask<IChannel> GetOrCreateChannelAsync(string channelName, CreateChannelOptions? options = null);
    ValueTask<IChannel> GetPublishChannelAsync(string exchange, CreateChannelOptions? options = null);
    ValueTask<IChannel> GetConsumeChannelAsync(string queue, string exchange, string routingKey, CreateChannelOptions? options = null);
    ConnectionFactory GetConnectionFactory();
}
