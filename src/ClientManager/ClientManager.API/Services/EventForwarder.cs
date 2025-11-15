using System.Text;
using System.Text.Json;
using ClientManager.Shared.Contracts.Events;
using ClientManager.Shared.Messaging;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClientManager.API.Services;

public class EventForwarder(IMessageBrokerFactory messageBrokerFactory, IHubContext<NotificationHub> hub, ILogger<EventForwarder> logger) : BackgroundService
{
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;
    readonly IHubContext<NotificationHub> _hub = hub;
    readonly ILogger<EventForwarder> _logger = logger;
    readonly IReadOnlyDictionary<string, Type> _eventTypesCache = DiscoverEventTypes().ToDictionary(t => t.Name, t => t);
    readonly IReadOnlyDictionary<string, Type> _interfaceCache = DiscoverResponseInterfaces().ToDictionary(t => t.Name, t => t);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var eventTypes = new[] { "ClientCreated" };
        foreach (var eventType in eventTypes)
            await SubscribeAsync(eventType, stoppingToken);
    }

    async Task SubscribeAsync(string queueName, CancellationToken cancellationToken)
    {
        var channel = await _messageBrokerFactory.GetConsumeChannelAsync(queueName);
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var eventType = _eventTypesCache[queueName];

            var envelope = JsonSerializer.Deserialize<MessageEnvelope>(body);
            ArgumentNullException.ThrowIfNull(envelope);

            var jsonElement = envelope.Payload;
            var messageEvent = jsonElement.Deserialize(eventType);

            var eventInterface = _interfaceCache[queueName];
            var responseDto = eventInterface.GetMethod("ToResponse")!.Invoke(messageEvent, null);
            _logger.LogInformation(
                "Forwarding event\n\t {@Event}\n\t [correlationId: {correlationId}, causationId: {causationId}]",
                responseDto,
                envelope.CorrelationId,
                envelope.CausationId
            );
            await _hub.Clients.All.SendAsync(responseDto!.GetType().Name, responseDto, cancellationToken);
            await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);
    }

    static IEnumerable<Type> DiscoverEventTypes() =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IEvent).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
            .ToList();

    static IEnumerable<Type> DiscoverResponseInterfaces() =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type.GetInterfaces().Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEventToResponse<>))
                && !type.IsAbstract
                && !type.IsInterface
            )
            .ToList();
}
