using RabbitMQ.Client;

namespace ClientManager.Shared.Messaging;

public interface IMessageBrokerFactory : IAsyncDisposable
{
    ValueTask<IConnection> GetConnectionAsync();
    ValueTask<IChannel> GetChannelAsync(string channelName, CreateChannelOptions? options = null);
    ConnectionFactory GetConnectionFactory();
}
