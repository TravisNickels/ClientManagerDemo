using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace ClientManager.Shared.Messaging;

public class RabbitMQMessageConsumer(IServiceScopeFactory serviceScopeFactory, IMessageBrokerFactory messageBrokerFactory, ILogger<RabbitMQMessageConsumer> logger)
    : IMessageConsumer
{
    readonly IServiceScopeFactory _scopeFactory = serviceScopeFactory;
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;
    readonly ILogger<RabbitMQMessageConsumer> _logger = logger;
    readonly ConcurrentDictionary<string, IChannel> _channels = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var messageTypes = DiscoverMessageTypes();

        return Task.CompletedTask;
    }

    static IEnumerable<Type> DiscoverMessageTypes() =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IMessage).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .ToList();
}
