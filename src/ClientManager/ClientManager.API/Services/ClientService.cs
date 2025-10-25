using System.Text.Json;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;

namespace ClientManager.API.Services;

public class ClientService(IQueuePublisher publisher) : IClientService
{
    IQueuePublisher _queuePublisher = publisher;

    public async Task<bool> SendCreateClientMessage(Client client, string queueName = "clients", string exchange = "client-manager", string routingKey = "")
    {
        ValidateClient(client);

        if (string.IsNullOrWhiteSpace(routingKey))
            routingKey = queueName;

        var body = JsonSerializer.SerializeToUtf8Bytes(client);
        try
        {
            await _queuePublisher.PublishAsync(queueName, body, exchange, routingKey);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to enqueue client message.", ex);
        }
    }

    void ValidateClient(Client client)
    {
        if (string.IsNullOrWhiteSpace(client.FirstName))
            throw new ArgumentException("Client must have a first name.", nameof(client.FirstName));
        if (string.IsNullOrWhiteSpace(client.LastName))
            throw new ArgumentException("Client must have a last name.", nameof(client.LastName));
        if (string.IsNullOrWhiteSpace(client.Email))
            throw new ArgumentException("Client must have an email.", nameof(client.Email));

        if (client.Id == Guid.Empty)
            client.Id = Guid.NewGuid();
    }
}
