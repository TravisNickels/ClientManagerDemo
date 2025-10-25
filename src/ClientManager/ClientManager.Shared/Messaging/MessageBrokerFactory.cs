using System.Buffers.Binary;
using System.Collections.Concurrent;
using ClientManager.Shared.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClientManager.Shared.Messaging;

public class MessageBrokerFactory(RabbitMQConnectionConfiguration rabbitMQConnectionConfiguration) : IMessageBroker
{
    IConnection? _connection;
    ConcurrentDictionary<string, IChannel> _channels = [];
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

    public async ValueTask<IChannel> GetChannelAsync(string channelName, CreateChannelOptions? options = null)
    {
        _connection = await GetConnectionAsync();

        // TODO: Determine if this is needed to override defaults or passed in options
        options ??= new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true, outstandingPublisherConfirmationsRateLimiter: new ThrottlingRateLimiter(1000));

        if (!_channels.TryGetValue(channelName, out var channel))
        {
            channel = await _connection.CreateChannelAsync(options);
            channel.BasicAcksAsync += OnAck;
            channel.BasicNacksAsync += OnNack;
            channel.BasicReturnAsync += OnBasicReturn;
            _channels.AddOrUpdate(channelName, channel, (key, oldValue) => channel);
        }

        return channel;
    }

    public async Task SendToQueue(string queueName, ReadOnlyMemory<byte> body, string exchange = "", string routingKey = "")
    {
        routingKey = string.IsNullOrWhiteSpace(routingKey) ? queueName : routingKey;
        exchange = string.IsNullOrWhiteSpace(exchange) ? "client-manager" : exchange;

        var channel = await GetChannelAsync(routingKey);
        await DeclareQueue(queueName, exchange, routingKey);

        //exchange = "Fail";
        //routingKey = "Boom";

        var props = new BasicProperties();
        ulong sequenceNumber = await channel.GetNextPublishSequenceNumberAsync();
        outstandingConfirms.AddLast(sequenceNumber);

        await channel.BasicPublishAsync(exchange, routingKey, mandatory: true, basicProperties: props, body);
    }

    async ValueTask DeclareQueue(string queueName, string exchange = "", string routingKey = "")
    {
        var channel = await GetChannelAsync(queueName);
        await channel.ExchangeDeclareAsync(exchange, "direct", durable: true, autoDelete: false, arguments: null);
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueBindAsync(queueName, exchange, routingKey);
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
        else
        {
            sequenceNumber = 0;
        }

        Console.WriteLine($"{DateTime.Now} [WARNING] message has been basic.return-ed: Exchange={args.Exchange}, RoutingKey={args.RoutingKey}, ReplyText={args.ReplyText}, SequenceNumber={sequenceNumber}");
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

    public ConnectionFactory GetConnectionFactory() => _connectionFactory;
}
