using ClientManager.API.Data;
using ClientManager.API.Models;

namespace ClientManager.API.Repositories;

public class ClientRepository(AppDbContext appDbContext) : IClientRepository
{
    AppDbContext _context = appDbContext;

    public async Task<Client> AddAsync(Client client)
    {
        var entry = _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return entry.Entity;
    }
}

