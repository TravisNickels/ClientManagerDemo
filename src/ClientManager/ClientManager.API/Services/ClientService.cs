using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Data;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Exceptions;

namespace ClientManager.API.Services;

public class ClientService(IMessagePublisher publisher, ReadOnlyAppDbContext readOnlyAppDbContext) : IClientService
{
    readonly IMessagePublisher _messagePublisher = publisher;
    readonly ReadOnlyAppDbContext _readOnlyAppDbContext = readOnlyAppDbContext;

    public async Task<CreateClient> SendCreateClientMessageAsync(CreateClient message)
    {
        try
        {
            await _messagePublisher.PublishAsync(message);
            return message;
        }
        catch (PublishException)
        {
            throw;
        }
    }

    public IEnumerable<Client> GetAllClients() => _readOnlyAppDbContext.Clients;

    //public async Task<Client?> GetClientByIdAsync(Guid id) => await _readOnlyAppDbContext.Clients.FirstOrDefaultAsync(c => c.Id == id);
    public async Task<Client?> GetClientByIdAsync(Guid id) => await _readOnlyAppDbContext.Clients.FindAsync(id);

    public async Task<ChangeClientArchiveStatus> SendChangeClientArchiveStatusMessageAsync(ChangeClientArchiveStatus message)
    {
        try
        {
            await _messagePublisher.PublishAsync(message);
            return message;
        }
        catch (PublishException)
        {
            throw;
        }
    }
}
