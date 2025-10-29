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

            await channel.BasicConsumeAsync(queueName, autoAck: true, consumer);
            _logger.LogInformation("Listening on queue: {queue}", queueName);
        }
    }

    static IEnumerable<Type> DiscoverMessageTypes() =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IMessage).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .ToList();
}
