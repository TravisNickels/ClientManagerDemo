namespace ClientManager.Shared.Messaging;

public interface IQueuePublisher
{
    Task PublishToQueueAsync(string queueName, ReadOnlyMemory<byte> body, string exchange, string routingKey);
}
