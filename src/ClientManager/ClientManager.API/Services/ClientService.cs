using ClientManager.API.Mappers;
using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Data;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using RabbitMQ.Client.Exceptions;

namespace ClientManager.API.Services;

public class ClientService(IMessagePublisher publisher, ReadOnlyAppDbContext readOnlyAppDbContext) : IClientService
{
    readonly IMessagePublisher _messagePublisher = publisher;
    readonly ReadOnlyAppDbContext _readOnlyAppDbContext = readOnlyAppDbContext;

    public async Task<CreateClient> SendCreateClientMessage(CreateClient message)
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

    public IEnumerable<Client> GetAllClients()
    {
        var clients = _readOnlyAppDbContext.Clients;

        return clients;
    }

    public Task<Client> GetClientById(Guid id)
    {
        throw new NotImplementedException();
    }
}
