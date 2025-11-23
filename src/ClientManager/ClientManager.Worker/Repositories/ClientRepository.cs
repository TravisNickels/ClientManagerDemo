using ClientManager.Shared.Data;
using ClientManager.Shared.Models;

namespace ClientManager.Worker.Repositories;

public class ClientRepository(AppDbContext appDbContext) : IClientRepository
{
    AppDbContext _context = appDbContext;

    public async Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Clients.AddAsync(client, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => await _context.Clients.FindAsync(id);

    public async Task UpdateArchiveStatus(Guid id, bool isArchived, CancellationToken cancellationToken)
    {
        var client = await _context.Clients.FindAsync([id], cancellationToken: cancellationToken) ?? throw new InvalidOperationException($"Client with id {id} not found.");
        client.IsArchived = isArchived;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
