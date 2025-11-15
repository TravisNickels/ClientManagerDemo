using System.Collections.Concurrent;
using System.Text;
using ClientManager.Shared.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClientManager.Worker.Messaging;

public class RabbitMQMessageConsumer(
    IServiceScopeFactory serviceScopeFactory,
    MessageConsumePipeline messageConsumePipeline,
    IMessageBrokerFactory messageBrokerFactory,
    MessageTypeRegistry messageTypeRegistry,
    ILogger<RabbitMQMessageConsumer> logger
) : IMessageConsumer
{
    readonly IServiceScopeFactory _scopeFactory = serviceScopeFactory;
    readonly MessageConsumePipeline _messageConsumePipeline = messageConsumePipeline;
    readonly IMessageBrokerFactory _messageBrokerFactory = messageBrokerFactory;

    readonly ILogger<RabbitMQMessageConsumer> _logger = logger;
    readonly ConcurrentDictionary<string, IChannel> _channels = new();

    readonly IReadOnlyDictionary<string, Type> _messageTypeCache = messageTypeRegistry.MessageTypeCache;

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
            var context = new MessageConsumeContext { RawJson = json };

            try
            {
                await _messageConsumePipeline.ExecuteAsync(context, DispatchToHandler, cancellationToken);
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

    public static async Task DispatchToHandler(MessageConsumeContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context.MessageType, nameof(context.MessageType));
        if (!context.Scope.HasValue)
        {
            throw new NullReferenceException($"Message scope for DI is not set for message type {context.MessageType.FullName}.");
        }

        var handlerType = typeof(IHandleMessage<>).MakeGenericType(context.MessageType);
        var handler =
            context.Scope.Value.ServiceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for message type {context.MessageType.FullName}.");
        var messageContext = context.MessageContext;

        if (handler is not null && context.Message is not null)
        {
            dynamic dynamicHandler = handler;
            await dynamicHandler.HandleAsync((dynamic)context.Message, messageContext, cancellationToken);
        }
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
}
