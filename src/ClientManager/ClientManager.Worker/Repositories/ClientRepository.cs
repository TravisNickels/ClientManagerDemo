using ClientManager.Shared.Data;
using ClientManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Clients.Include(c => c.Phones).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task UpdateArchiveStatus(Guid id, bool isArchived, CancellationToken cancellationToken)
    {
        var client = await _context.Clients.FindAsync([id], cancellationToken: cancellationToken) ?? throw new InvalidOperationException($"Client with id {id} not found.");
        client.IsArchived = isArchived;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Client updatedClient, CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.Clear();

        var existing =
            await _context.Clients.Include(c => c.Phones).FirstOrDefaultAsync(c => c.Id == updatedClient.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Client with id {updatedClient.Id} not found.");

        existing.FirstName = updatedClient.FirstName;
        existing.LastName = updatedClient.LastName;
        existing.Email = updatedClient.Email;
        existing.IsArchived = updatedClient.IsArchived;

        var existingPhones = existing.Phones.ToList();
        var updatedPhones = updatedClient.Phones.ToList();

        // Remove phones that are no longer present
        foreach (var phone in existingPhones)
        {
            if (!updatedPhones.Any(p => p.Id == phone.Id))
                _context.Phones.Remove(phone);
        }

        // Update existing phones and add new ones
        foreach (var phone in updatedPhones)
        {
            var existingPhone = existingPhones.FirstOrDefault(p => p.Id == phone.Id);
            if (existingPhone != null)
            {
                existingPhone.Number = phone.Number;
                existingPhone.Type = phone.Type;
            }
            else
            {
                _context.Phones.Add(
                    new Phone
                    {
                        Id = phone.Id == Guid.Empty ? Guid.NewGuid() : phone.Id,
                        ClientId = existing.Id,
                        Number = phone.Number,
                        Type = phone.Type
                    }
                );
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
