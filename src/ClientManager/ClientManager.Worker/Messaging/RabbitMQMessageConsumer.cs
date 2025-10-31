using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading;
using ClientManager.Shared.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClientManager.Worker.Messaging;

public class RabbitMQMessageConsumer(IServiceScopeFactory serviceScopeFactory, IMessageBrokerFactory messageBrokerFactory, ILogger<RabbitMQMessageConsumer> logger)
    : IMessageConsumer
{
    readonly IServiceScopeFactory _scopeFactory = serviceScopeFactory;
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;
    readonly ILogger<RabbitMQMessageConsumer> _logger = logger;
    readonly ConcurrentDictionary<string, IChannel> _channels = new();
    readonly IReadOnlyDictionary<string, Type> _messageTypeCache = DiscoverMessageTypes().ToDictionary(t => t.Name, t => t);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var messageType in _messageTypeCache.Values)
        {
            await SubscribeAsync(messageType.Name, cancellationToken);
        }
    }

    async Task SubscribeAsync(string queueName, CancellationToken cancellationToken)
    {
        var channel = await _messageBrokerFactory.GetConsumeChannelAsync(queueName);
        _channels.AddOrUpdate(queueName, channel, (key, oldValue) => channel);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Received message from {queue}: {json}", queueName, json);

            try
            {
                await DispatchToHandlers(queueName, json, cancellationToken);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message from {queue}", queueName);
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
            }
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);
        _logger.LogInformation("Listening on queue: {queue}", queueName);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var (queue, channel) in _channels)
        {
            await channel.BasicCancelAsync(queue, cancellationToken: cancellationToken);
            await channel.CloseAsync(cancellationToken);
        }

        _logger.LogInformation("All channels closed.");
    }

    static IEnumerable<Type> DiscoverMessageTypes() =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .ToList();

    async Task DispatchToHandlers(string queueName, string json, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        if (!_messageTypeCache.TryGetValue(queueName, out var messageType))
        {
            _logger.LogWarning("No message type found for queue: {queue}", queueName);
            return;
        }

        var message = JsonSerializer.Deserialize(json, messageType);

        // Find handlers registered for that type
        var handlerType = typeof(IHandlerMessage<>).MakeGenericType(messageType);
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is not null && message is not null)
            {
                dynamic dynamicHandler = handler;
                await dynamicHandler.HandleAsync((dynamic)message, cancellationToken);
            }
        }
    }
}
