using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Models;

namespace ClientManager.API.Services;

public interface IClientService
{
    Task<CreateClient> SendCreateClientMessage(CreateClient message);
    Task<Client> GetClientById(Guid id);
}
