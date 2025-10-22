using ClientManagerAPI.Models;
using ClientManagerAPI.Repositories;

namespace ClientManagerAPI.Services;

public class ClientService(IClientRepository clientRepository)
{
    IClientRepository _clientRepository = clientRepository;

    public async Task<Client> CreateClientAsync(Client client)
    {
        if (client.Id == Guid.Empty)
            client.Id = Guid.NewGuid();

        return await _clientRepository.AddAsync(client);
    }
}

