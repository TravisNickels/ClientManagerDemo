using ClientManager.Shared.Models;

namespace ClientManager.API.Services;

public interface IClientService
{
    Task<bool> SendCreateClientMessage(Client client, string queueName, string exchange = "", string routingKey = "");
}
