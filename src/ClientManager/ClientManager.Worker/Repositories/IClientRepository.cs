using ClientManager.Shared.Models;

namespace ClientManager.Worker.Repositories;

public interface IClientRepository
{
    Task<Client> AddAsync(Client client);
}

