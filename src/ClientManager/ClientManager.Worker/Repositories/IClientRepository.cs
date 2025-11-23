using ClientManager.Shared.Models;

namespace ClientManager.Worker.Repositories;

public interface IClientRepository
{
    Task<Client> AddAsync(Client client, CancellationToken cancellationToken = default);
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateArchiveStatus(Guid id, bool isArchived, CancellationToken cancellationToken = default);
    Task UpdateAsync(Client client, CancellationToken cancellationToken = default);
}
