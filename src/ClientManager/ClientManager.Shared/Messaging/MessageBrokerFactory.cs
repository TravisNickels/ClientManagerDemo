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
    const string exchangeName = "client-manager";
    readonly LinkedList<ulong> outstandingConfirms = new();
    readonly SemaphoreSlim semaphore = new(1, 1);
    readonly TaskCompletionSource<bool> allMessagesConfirmedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    const int MESSAGE_COUNT = 10000;
    int confirmedCount = 0;
    const bool debug = true;

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

    public async ValueTask<IChannel> GetOrCreateChannelAsync(string channelName, CreateChannelOptions? options = null)
    {
        _connection = await GetConnectionAsync();

        if (!_channels.TryGetValue(channelName, out var channel))
        {
            channel = await _connection.CreateChannelAsync(options);
            _channels.AddOrUpdate(channelName, channel, (key, oldValue) => channel);
        }

        return channel;
    }

    public async ValueTask<IChannel> GetPublishChannelAsync(string exchange, CreateChannelOptions? options = null)
    {
        exchange = string.IsNullOrWhiteSpace(exchange) ? exchangeName : exchange;

        if (!_channels.TryGetValue(exchange, out IChannel? existing) || existing is null)
        {
            options ??= new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true,
                outstandingPublisherConfirmationsRateLimiter: new ThrottlingRateLimiter(1000)
            );
            var channel = await GetOrCreateChannelAsync(exchange, options);
            await channel.ExchangeDeclareAsync(exchange, "direct", durable: true, autoDelete: false, arguments: null);
            channel.BasicAcksAsync += OnAck;
            channel.BasicNacksAsync += OnNack;
            channel.BasicReturnAsync += OnBasicReturn;
            return channel;
        }

        return existing;
    }

    public async ValueTask<IChannel> GetConsumeChannelAsync(string queueName, string exchange = "", string routingKey = "", CreateChannelOptions? options = null)
    {
        exchange = string.IsNullOrWhiteSpace(exchange) ? exchangeName : exchange;
        routingKey = string.IsNullOrWhiteSpace(routingKey) ? queueName : routingKey;

        if (!_channels.TryGetValue(queueName, out IChannel? existing) || existing is null)
        {
            var channel = await GetOrCreateChannelAsync(queueName, options);
            await channel.ExchangeDeclareAsync(exchange, "direct", durable: true, autoDelete: false, arguments: null);
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await channel.QueueBindAsync(queueName, exchange, routingKey);
            return channel;
        }

        return existing;
    }

    public ConnectionFactory GetConnectionFactory() => _connectionFactory;

    async Task OnAck(object sender, BasicAckEventArgs args) => await CleanOutstandingConfirms(args.DeliveryTag, args.Multiple);

    async Task OnNack(object sender, BasicNackEventArgs args) => await CleanOutstandingConfirms(args.DeliveryTag, args.Multiple);

    Task OnBasicReturn(object sender, BasicReturnEventArgs args)
    {
        var props = args.BasicProperties;
        ulong sequenceNumber = 0;

        if (props.Headers != null && props.Headers.TryGetValue(Constants.PublishSequenceNumberHeader, out var headerValue))
        {
            sequenceNumber = headerValue switch
            {
                byte[] bytes => BinaryPrimitives.ReadUInt64BigEndian(bytes),
                long longValue => (ulong)longValue,
                int intValue => (ulong)intValue,
                _ => 0
            };
        }

        Console.WriteLine(
            $"{DateTime.Now} [WARNING] message has been basic.return-ed: Exchange={args.Exchange}, RoutingKey={args.RoutingKey}, ReplyText={args.ReplyText}, SequenceNumber={sequenceNumber}"
        );
        return Task.CompletedTask;
    }

    async Task CleanOutstandingConfirms(ulong deliveryTag, bool multiple)
    {
        if (debug)
            Console.WriteLine($"{DateTime.Now} [DEBUG] confirming message: {deliveryTag} (multiple: {multiple})");

        await semaphore.WaitAsync();
        try
        {
            if (multiple)
            {
                while (outstandingConfirms.First is { } node && node.Value <= deliveryTag)
                {
                    outstandingConfirms.RemoveFirst();
                    confirmedCount++;
                }
            }
            else
            {
                outstandingConfirms.Remove(deliveryTag);
                confirmedCount++;
            }
        }
        finally
        {
            semaphore.Release();
        }

        if (outstandingConfirms.Count == 0 || confirmedCount == MESSAGE_COUNT)
        {
            allMessagesConfirmedTcs.TrySetResult(true);
        }
    }

    public async ValueTask<IChannel> DeclareAndBindQueue(string queueName, string exchange = exchangeName, string routingKey = "")
    {
        routingKey = string.IsNullOrWhiteSpace(routingKey) ? queueName : routingKey;

        var channel = await GetOrCreateChannelAsync(queueName);
        await channel.ExchangeDeclareAsync(exchange, "direct", durable: true, autoDelete: false, arguments: null);
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueBindAsync(queueName, exchange, routingKey);

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

        GC.SuppressFinalize(this);
    }
}
