using System.Collections.Concurrent;
using System.Text;
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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var messageTypes = DiscoverMessageTypes();

        foreach (var messageType in messageTypes)
        {
            var queueName = messageType.Name;
            var channel = await _messageBrokerFactory.GetConsumeChannelAsync(messageType.Name);
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += (_, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation("Received message from {queue}: {json}", queueName, json);
                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);
            _logger.LogInformation("Listening on queue: {queue}", queueName);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var channel in _channels.Values)
        {
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

        // Find message type by queue name
        var messageType = DiscoverMessageTypes().FirstOrDefault(t => t.Name == queueName);
        if (messageType is null)
        {
            _logger.LogWarning("No message type found for queue: {queue}", queueName);
            return;
        }

        var message = JsonSerializer.Deserialize(json, messageType);

        // Find handlers registered for that type
        var handlerType = typeof(IMessageHandler<>).MakeGenericType(messageType);
        var handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("HandleAsync")!;
            await (Task)method.Invoke(handler, [message!, cancellationToken])!;
        }
    }
}
