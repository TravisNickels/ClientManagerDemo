using System.Buffers.Binary;
using System.Collections.Concurrent;
using ClientManager.Shared.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClientManager.Shared.Messaging;

public class MessageBrokerFactory(RabbitMQConnectionConfiguration rabbitMQConnectionConfiguration) : IMessageBrokerFactory
{
    IConnection? _connection;
    readonly ConcurrentDictionary<string, IChannel> _channels = [];

    readonly ConnectionFactory _connectionFactory =
        new()
        {
            Uri = new Uri(rabbitMQConnectionConfiguration.Url),
            Port = rabbitMQConnectionConfiguration.AmqpPort,
            VirtualHost = rabbitMQConnectionConfiguration.VirtualHost,
            UserName = rabbitMQConnectionConfiguration.Username,
            Password = rabbitMQConnectionConfiguration.Password
        };

    public async ValueTask<IConnection> GetConnectionAsync()
    {
        if (_connection != null && _connection.IsOpen)
            return _connection;

        _connection = await _connectionFactory.CreateConnectionAsync();
        return _connection;
    }

    public async ValueTask<IChannel> GetChannelAsync(string channelName, CreateChannelOptions? options = null)
    {
        _connection = await GetConnectionAsync();

        if (!_channels.TryGetValue(channelName, out var channel))
        {
            channel = await _connection.CreateChannelAsync(options);
            _channels.AddOrUpdate(channelName, channel, (key, oldValue) => channel);
        }

        return channel;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var channel in _channels.Values)
        {
            await channel.CloseAsync();
        }
        _channels.Clear();
        if (_connection != null && _connection.IsOpen)
        {
            await _connection.CloseAsync();
        }
    }

    public ConnectionFactory GetConnectionFactory() => _connectionFactory;
}
