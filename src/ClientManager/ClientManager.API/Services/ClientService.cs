using System.Text.Json;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;

namespace ClientManager.API.Services;

public class ClientService(IMessageBroker broker) : IClientService
{
    IMessageBroker _messageBroker = broker;

    public async Task<bool> SendCreateClientMessage(Client client, string queueName = "test-queue")
    {
        if (client.Id == Guid.Empty)
            client.Id = Guid.NewGuid();

        var body = JsonSerializer.SerializeToUtf8Bytes(client);
        try
        {
            await _messageBroker.SendToQueue(queueName, body, "MyExchange", "CorrectKey");
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}
