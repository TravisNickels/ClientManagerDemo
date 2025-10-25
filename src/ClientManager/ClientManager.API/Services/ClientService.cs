using System.Text.Json;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;

namespace ClientManager.API.Services;

public class ClientService(IQueuePublisher publisher) : IClientService
{
    IQueuePublisher _queuePublisher = publisher;

    public async Task<bool> SendCreateClientMessage(Client client, string queueName = "test-queue")
    {
        if (client.Id == Guid.Empty)
            client.Id = Guid.NewGuid();

        var body = JsonSerializer.SerializeToUtf8Bytes(client);
        try
        {
            await _queuePublisher.PublishToQueueAsync(queueName, body, "MyExchange", "CorrectKey");
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
