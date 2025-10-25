namespace ClientManager.Shared.Messaging;

public interface IQueuePublisher
{
    Task PublishAsync(string queueName, ReadOnlyMemory<byte> body, string exchange, string routingKey);
}
