using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Models;

namespace ClientManager.API.Services;

public interface IClientService
{
    Task<CreateClient> SendCreateClientMessageAsync(CreateClient message);
    Task<Client?> GetClientByIdAsync(Guid id);
    IEnumerable<Client> GetAllClients();
    Task SendChangeClientArchiveStatusMessageAsync(ChangeClientArchiveStatus message);
    Task SendUpdateClientMessageAsync(UpdateClient message);
    Task SendDeleteClientMessageAsync(DeleteClient message);
}
