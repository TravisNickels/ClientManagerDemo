using ClientManager.API.Models;

namespace ClientManager.API.Repositories;

public interface IClientRepository
{
    Task<Client> AddAsync(Client client);
}

