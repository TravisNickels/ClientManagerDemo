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

    public async Task<Client?> GetClientByIdAsync(Guid id) => await _readOnlyAppDbContext.Clients.Include(c => c.Phones).FirstOrDefaultAsync(c => c.Id == id);

    public async Task SendChangeClientArchiveStatusMessageAsync(UpdateClientArchiveStatus message)
    {
        try
        {
            await _messagePublisher.PublishAsync(message);
        }
        catch (PublishException)
        {
            throw;
        }
    }

    public async Task SendUpdateClientMessageAsync(UpdateClient message)
    {
        try
        {
            await _messagePublisher.PublishAsync(message);
        }
        catch (PublishException)
        {
            throw;
        }
    }

    public async Task SendDeleteClientMessageAsync(DeleteClient message)
    {
        try
        {
            await _messagePublisher.PublishAsync(message);
        }
        catch (PublishException)
        {
            throw;
        }
    }
}
