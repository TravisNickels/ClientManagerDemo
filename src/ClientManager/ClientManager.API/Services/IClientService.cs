using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Models;

namespace ClientManager.API.Services;

public interface IClientService
{
    Task<CreateClient> SendCreateClientMessage(CreateClient message, string queueName = "", string exchange = "", string routingKey = "");
    Task<Client> GetClientById(Guid id);
}
