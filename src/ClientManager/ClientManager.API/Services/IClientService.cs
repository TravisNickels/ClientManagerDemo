using ClientManager.Shared.Models;

namespace ClientManager.API.Services;

public interface IClientService
{
    Task<Client> SendCreateClientMessage(Client client, string queueName = "", string exchange = "", string routingKey = "");
    Task<Client> GetClientById(Guid id);
}
