using System.Text.Json;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;

namespace ClientManager.API.Services;

public class ClientService(IQueuePublisher publisher) : IClientService
{
    IQueuePublisher _queuePublisher = publisher;

    public async Task<bool> SendCreateClientMessage(Client client, string queueName = "test-queue")
    {
        if (string.IsNullOrWhiteSpace(client.FirstName))
            throw new ArgumentException("Client must have a first name.", nameof(client.FirstName));

        if (string.IsNullOrWhiteSpace(client.LastName))
            throw new ArgumentException("Client must have a last name.", nameof(client.LastName));

        if (string.IsNullOrWhiteSpace(client.Email))
            throw new ArgumentException("Client must have an email.", nameof(client.Email));

        if (client.Id == Guid.Empty)
            client.Id = Guid.NewGuid();

        var body = JsonSerializer.SerializeToUtf8Bytes(client);
        try
        {
            await _queuePublisher.PublishAsync(queueName, body, "MyExchange", "CorrectKey");
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to enqueue client message.", ex);
        }
    }
}
