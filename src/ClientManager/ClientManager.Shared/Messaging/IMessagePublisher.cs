namespace ClientManager.Shared.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(string queueName, ReadOnlyMemory<byte> body, string exchange = "", string routingKey = "");
}
