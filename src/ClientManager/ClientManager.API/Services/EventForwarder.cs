using System.Text;
using ClientManager.Shared.Contracts.Events;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClientManager.API.Services;

public class EventForwarder(IMessageBrokerFactory messageBrokerFactory, IHubContext<NotificationHub> hub, ILogger<EventForwarder> logger) : BackgroundService
{
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;
    readonly IHubContext<NotificationHub> _hub = hub;
    readonly ILogger<EventForwarder> _logger = logger;

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
            _logger.LogInformation("Forwarding event {event}", body);
            await _hub.Clients.All.SendAsync("eventReceived", body, cancellationToken);
            await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer);
    }
}
