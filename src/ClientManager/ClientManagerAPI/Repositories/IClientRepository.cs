using ClientManagerAPI.Models;

namespace ClientManagerAPI.Repositories;

public interface IClientRepository
{
    Task<Client> AddAsync(Client client);
}

